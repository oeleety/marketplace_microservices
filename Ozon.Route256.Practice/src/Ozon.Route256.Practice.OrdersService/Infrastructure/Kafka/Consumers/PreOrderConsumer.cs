using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Ozon.Route256.Practice.OrdersService.CachedClients;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class PreOrderConsumer : ConsumerBackgroundService<long, string>
{
    private readonly ILogger<PreOrderConsumer> _logger;
    private readonly CachedCustomersClient _cachedCustomersClient;
    private readonly RedisOrdersRepository _redisOrdersRepository;
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
    }

    protected override async Task HandleAsync(
        ConsumeResult<long, string> message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HANDLING MESSAGE FROM KAFKA {message.Message.Value}", message.Message.Value);
        var value = message.Message.Value;
        var preOrder = JsonSerializer.Deserialize<PreOrder>(value, _jsonSerializerOptions);
        var customer = await _cachedCustomersClient.GetCustomerByIdAsync(preOrder.Customer.Id, cancellationToken);
        //var customer = await _cachedCustomersClient.GetCustomerByIdAsync(1, cancellationToken); // for testing
        if (customer is null)
        {
            _logger.LogError("Couldn't find a customer in CustomerService with id = {preOrder.Customer.Id} " +
                "from a preorder with key {message.Message.Key}", preOrder.Customer.Id, message.Message.Key);
            return;
        }
        var orderEntity = From(preOrder, customer);
        await _redisOrdersRepository.AddOrderAsync(orderEntity, cancellationToken);
        _logger.LogInformation("New preorder created");
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
}