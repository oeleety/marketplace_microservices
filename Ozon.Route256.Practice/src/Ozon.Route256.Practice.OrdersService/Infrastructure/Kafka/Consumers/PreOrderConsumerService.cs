using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Ozon.Route256.Practice.OrdersService.CachedClients;
using System.Collections.Concurrent;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;
using Ozon.Route256.Practice.OrdersService.GrpcClients;
using Bogus;
using Ozon.Route256.Practice.OrdersService.Configuration;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public sealed class PreOrderConsumerService : ConsumerBackgroundService<long, string>
{
    private const int DeliveryArea = 5000000; // meters

    private static ConcurrentDictionary<string, (double latitude, double Longitude)> _depotsByRegion;
    private static readonly Faker Faker = new();

    private readonly ILogger<PreOrderConsumerService> _logger;
    private readonly CachedCustomersClient _cachedCustomersClient;
    private readonly IRedisOrdersRepository _redisOrdersRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly INewOrderProducer _producer;
    private readonly CustomersServiceClient _customersServiceTest;
    private bool _isCustomersFilledTest = false;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public PreOrderConsumerService(
        IServiceProvider serviceProvider,
        INewOrderProducer producer,
        CustomersServiceClient customersService,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<PreOrderConsumerService> logger)
        : base(serviceProvider,
            kafkaSettings,
            logger)
    {
        _logger = logger;
        _redisOrdersRepository = _scope.ServiceProvider.GetRequiredService<IRedisOrdersRepository>();
        _cachedCustomersClient = _scope.ServiceProvider.GetRequiredService<CachedCustomersClient>();
        _ordersRepository = _scope.ServiceProvider.GetRequiredService<IOrdersRepository>();
        _producer = producer;
        _customersServiceTest = customersService;
        KafkaConsumer = new KafkaConsumer(_kafkaSettings, "pre_orders_group");
    }

    protected override string TopicName { get; } = "pre_orders";
    protected override IKafkaConsumer<long, string> KafkaConsumer { get; }

    protected override async Task HandleAsync(
        ConsumeResult<long, string> message, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Handling messages from kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);
        var value = message.Message.Value;
        var preOrder = JsonSerializer.Deserialize<PreOrder>(value, _jsonSerializerOptions);

        int count = 5;
        if (!_isCustomersFilledTest) // for testing. there's no customers in CustomersService todo
        {
            for( var i = 1; i <= count; i++)
            {
                var randomCustomer = CreateRandomCustomer(i);
                await _customersServiceTest.CreateCustomerAsync(randomCustomer, cancellationToken);
                _logger.LogInformation("Customers servise contains customers");
            }
            _isCustomersFilledTest = true;
        }
        var customerId = new Random().Next(1, count + 1);// preOrder.Customer.Id
        var customer = await _cachedCustomersClient.GetCustomerByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogError("Couldn't find a customer in CustomerService with id = {customerId} " +
                "from a preorder with key {message.Message.Key}", customerId, message.Message.Key);
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
    /// Distance between two points in meters (GeoCoordinate)
    /// </summary>
    /// <param name="address1"></param>
    /// <param name="address2"></param>
    /// <returns></returns>
    private static double GetDistance((double latitude, double longitude) address1, (double latitude, double longitude) address2)
    {
        var oneDegree = Math.PI / 180.0;
        var d1 = address1.latitude * oneDegree;
        var num1 = address1.longitude * oneDegree;
        var d2 = address2.latitude * oneDegree;
        var num2 = address2.longitude * oneDegree - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        var earthR = 6376500.0;
        return earthR * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }

    private static Practice.Proto.Customer CreateRandomCustomer(int id = -1)
    {
        var adresses = Enumerable.Range(0, Faker.Random.Int(1, 3)).
                Select(_ => new Practice.Proto.Address
                {
                    Apartment = Faker.Random.Int(9, 999).ToString(),
                    Building = Faker.Address.BuildingNumber(),
                    Region = Faker.Address.State(),
                    City = Faker.Address.City(),
                    Street = Faker.Address.StreetName(),
                    Latitude = Faker.Address.Latitude(),
                    Longitude = Faker.Address.Longitude()
                });
        return new Practice.Proto.Customer
        {
            Id = id == -1 ? Faker.Random.Int(100, 10000) : id,
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            MobileNumber = Faker.Phone.PhoneNumber(),
            Email = Faker.Person.Email,
            Addressed = { adresses },
            DefaultAddress = adresses.First()
        };
    }
}