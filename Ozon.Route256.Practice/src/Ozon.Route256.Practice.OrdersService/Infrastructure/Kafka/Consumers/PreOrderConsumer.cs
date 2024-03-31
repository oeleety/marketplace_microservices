using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Ozon.Route256.Practice.OrdersService.CachedClients;
using System.Collections.Concurrent;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class PreOrderConsumer : ConsumerBackgroundService<long, string>
{
    private const int DeliveryArea = 7000000; // meters

    private static ConcurrentDictionary<string, (double latitude, double Longitude)> _depotsByRegion;

    private readonly ILogger<PreOrderConsumer> _logger;
    private readonly CachedCustomersClient _cachedCustomersClient;
    private readonly RedisOrdersRepository _redisOrdersRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly INewOrderProducer _producer;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public PreOrderConsumer(
        IServiceProvider serviceProvider,
        IKafkaDataProvider<long, string> kafkaDataProvider,
        INewOrderProducer producer,
        ILogger<PreOrderConsumer> logger)
        : base(serviceProvider, kafkaDataProvider, logger)
    {
        _logger = logger;
        _redisOrdersRepository = _scope.ServiceProvider.GetRequiredService<RedisOrdersRepository>();
        _cachedCustomersClient = _scope.ServiceProvider.GetRequiredService<CachedCustomersClient>();
        _ordersRepository = _scope.ServiceProvider.GetRequiredService<IOrdersRepository>();
        _producer = producer;
    }

    protected override string TopicName { get; } = "pre_orders";

    protected override async Task HandleAsync(
        ConsumeResult<long, string> message, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Handling messages from kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);
        var value = message.Message.Value;
        var preOrder = JsonSerializer.Deserialize<PreOrder>(value, _jsonSerializerOptions);
        var customer = await _cachedCustomersClient.GetCustomerByIdAsync(preOrder.Customer.Id, cancellationToken);
        //var customer = await _cachedCustomersClient.GetCustomerByIdAsync(1, cancellationToken); // todo for testing(create customer with id =1 via postman)
        if (customer is null)
        {
            _logger.LogError("Couldn't find a customer in CustomerService with id = {preOrder.Customer.Id} " +
                "from a preorder with key {message.Message.Key}", preOrder.Customer.Id, message.Message.Key);
            return;
        }
        var orderEntity = From(preOrder, customer);
        var isExist = await _redisOrdersRepository.IsExistAsync(orderEntity.Id, cancellationToken);

        if (isExist)
        {
            _logger.LogError("Coudn't handle preorder with id = {orderEntity.Id} because repository already contains an order with the same Id.", orderEntity.Id);
            return;
        }
        await _redisOrdersRepository.AddOrderAsync(orderEntity, cancellationToken);
        _logger.LogInformation("Preorder added into redis");

        if (await ValidatePreOrderAsync(preOrder, cancellationToken))
        {
            await _producer.ProduceAsync(new[] { orderEntity.Id }, cancellationToken);
        }
    }

    private async Task<bool> ValidatePreOrderAsync(
        PreOrder preOrder,
        CancellationToken cancellationToken)
    {
        if (!_depotsByRegion?.Any() ?? true)
        {
            _depotsByRegion = await _ordersRepository.GetRegionsWithDepots(cancellationToken);
        }
        cancellationToken.ThrowIfCancellationRequested();

        var region = preOrder.Customer.Address.Region;
        if (!_depotsByRegion.TryGetValue(region, out (double latitude, double Longitude) depot))
        {
            var rand = new Random();
            depot = _depotsByRegion.ElementAt(rand.Next(0, _depotsByRegion.Count)).Value; // todo for testing purposes 
        }
        var distance = GetDistance(depot, (preOrder.Customer.Address.Latitude, preOrder.Customer.Address.Longitude));
        var isValid = distance <= DeliveryArea;
        _logger.LogInformation("Preorder with id = {preOrder.Id} has validation result = {isValid}. Distance = {distance}", preOrder.Id, isValid, distance);
        return isValid;
    }

    private static OrderEntity From(PreOrder preOrder, Practice.Proto.Customer customer) =>
        new(
            preOrder.Id,
            DataAccess.Entities.OrderStatusEntity.PreOrder,
            From(preOrder.Source), 
            customer.Id,
            customer.FirstName + " " + customer.LastName, 
            customer.MobileNumber,
            From(preOrder.Customer.Address),
            preOrder.Goods.Sum(g => g.Quantity),
            preOrder.Goods.Sum(g => g.Price),
            preOrder.Goods.Sum(g => g.Weight),
            DateTime.UtcNow,
            new RegionEntity(1, preOrder.Customer.Address.Region) // todo change contract to just a name 
        );

    private static OrderTypeEntity From(OrderSource source) => source switch
    {
        OrderSource.WebSite => OrderTypeEntity.Web,
        OrderSource.Mobile => OrderTypeEntity.Mobile,
        OrderSource.Api => OrderTypeEntity.Api,
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };

    private static AddressEntity From(Address a) => new(
        a.Region,
        a.City,
        a.Street,
        a.Building,
        a.Apartment,
        a.Latitude,
        a.Longitude);

    /// <summary>
    /// Distance between two points in meters
    /// </summary>
    /// <param name="address1"></param>
    /// <param name="address2"></param>
    /// <returns></returns>
    private static double GetDistance((double latitude, double longitude) address1, (double latitude, double longitude) address2)
    {
        var d1 = address1.latitude * (Math.PI / 180.0);
        var num1 = address1.longitude * (Math.PI / 180.0);
        var d2 = address2.latitude * (Math.PI / 180.0);
        var num2 = address2.longitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}