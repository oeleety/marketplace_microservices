using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Ozon.Route256.Practice.OrdersService.CachedClients;
using System.Collections.Concurrent;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class PreOrderConsumer : ConsumerBackgroundService<long, string>
{
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
        ILogger<PreOrderConsumer> logger)
        : base(serviceProvider, kafkaDataProvider, logger)
    {
        _logger = logger;
        _redisOrdersRepository = _scope.ServiceProvider.GetRequiredService<RedisOrdersRepository>();
        _cachedCustomersClient = _scope.ServiceProvider.GetRequiredService<CachedCustomersClient>();
        _ordersRepository = _scope.ServiceProvider.GetRequiredService<IOrdersRepository>();
    }

    protected override async Task HandleAsync( // todo
        ConsumeResult<long, string> message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HANDLING MESSAGE FROM KAFKA {message.Message.Value}", message.Message.Value);
        var value = message.Message.Value;
        var preOrder = JsonSerializer.Deserialize<PreOrder>(value, _jsonSerializerOptions);
        //var customer = await _cachedCustomersClient.GetCustomerByIdAsync(preOrder.Customer.Id, cancellationToken);
        var customer = await _cachedCustomersClient.GetCustomerByIdAsync(1, cancellationToken); // for testing todo
        if (customer is null)
        {
            _logger.LogError("Couldn't find a customer in CustomerService with id = {preOrder.Customer.Id} " +
                "from a preorder with key {message.Message.Key}", preOrder.Customer.Id, message.Message.Key);
            return;
        }
        if (!_depotsByRegion?.Any() ?? true)
        {
            _depotsByRegion = await _ordersRepository.GetRegionsWithDepots(cancellationToken);
        }
        var orderEntity = From(preOrder, customer);
        if(!await _redisOrdersRepository.IsExist(orderEntity, cancellationToken))
        {
            await _redisOrdersRepository.AddOrderAsync(orderEntity, cancellationToken);
            _logger.LogInformation("New preorder created");
            if (ValidatePreOrder(preOrder))
            {
                await _producer.ProduceAsync(new[] { orderEntity.Id }, cancellationToken);
                _logger.LogInformation("New preorder produced into kafka");
            }
        }
    }

    private static NewOrder From(PreOrder preOrder, Practice.Proto.Customer customer) => new ()
        {
            Id = preOrder.Id, 
            Source = preOrder.Source,
            CustomerId = customer.Id,
            CustomerFullName = customer.FirstName + " " + customer.LastName,
            CustomerMobileNumber = customer.MobileNumber,
            DeliveryAddress = preOrder.Customer.Address,
            Goods = preOrder.Goods
        };

    private static bool ValidatePreOrder(PreOrder preOrder)
    {
        var lon = preOrder.Customer.Address.Longitude;
        var lat = preOrder.Customer.Address.Latitude;
        var region = preOrder.Customer.Address.Region;
        // todo region converter
        if(_depotsByRegion.TryGetValue(region, out (double latitude, double Longitude) depot))
        {
            return true; // todo
        }
        return false;
    }
}