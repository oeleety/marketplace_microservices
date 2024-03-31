using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public class OrdersEventsConsumer : ConsumerBackgroundService<long, string>
{
    private readonly ILogger<OrdersEventsConsumer> _logger;
    private readonly IRedisOrdersRepository _redisOrdersRepository;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public OrdersEventsConsumer(
        IServiceProvider serviceProvider,
        IKafkaDataProvider<long, string> kafkaDataProvider,
        ILogger<OrdersEventsConsumer> logger)
        : base(serviceProvider, kafkaDataProvider, logger)
    {
        _logger = logger;
        _redisOrdersRepository = _scope.ServiceProvider.GetRequiredService<IRedisOrdersRepository>();
    }

    protected override string TopicName { get; } = "orders_events";

    protected override async Task HandleAsync(
        ConsumeResult<long, string> message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Handling messages from kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);

        var value = message.Message.Value;
        var orderEvent= JsonSerializer.Deserialize<OrderEvent>(value, _jsonSerializerOptions);

        var order = await _redisOrdersRepository.FindAsync(orderEvent.Id, cancellationToken);
        if (order is null)
        {
            _logger.LogError("No orders with id={orderEvent.Id} is found in repository. Couldn't handle event.", orderEvent.Id);
        }
        else
        {
            cancellationToken.ThrowIfCancellationRequested();

            var updOrder = order with { OrderStatus = From(orderEvent.NewState) };
            await _redisOrdersRepository.UpdateAsync(updOrder, cancellationToken);
            _logger.LogInformation("Order id = {orderEvent.Id} status updated", orderEvent.Id);
        }
    }

    private static DataAccess.Entities.OrderStatusEntity From(OrderStatusEntity eventStatus) => eventStatus switch
    {
        OrderStatusEntity.Created => DataAccess.Entities.OrderStatusEntity.Created,
        OrderStatusEntity.SentToCustomer => DataAccess.Entities.OrderStatusEntity.SentToCustomer,
        OrderStatusEntity.Delivered => DataAccess.Entities.OrderStatusEntity.Delivered,
        OrderStatusEntity.Lost => DataAccess.Entities.OrderStatusEntity.Lost,
        OrderStatusEntity.Cancelled => DataAccess.Entities.OrderStatusEntity.Cancelled,

        _ => throw new ArgumentOutOfRangeException(nameof(eventStatus), eventStatus, null)
    };
}