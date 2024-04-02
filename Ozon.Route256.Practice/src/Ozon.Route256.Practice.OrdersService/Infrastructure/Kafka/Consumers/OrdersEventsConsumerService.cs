using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Ozon.Route256.Practice.OrdersService.Configuration;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public sealed class OrdersEventsConsumerService : ConsumerBackgroundService<long, string>
{
    private readonly ILogger<OrdersEventsConsumerService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    private IRedisOrdersRepository _redisOrdersRepository;

    public OrdersEventsConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<OrdersEventsConsumerService> logger)
        : base(serviceProvider, kafkaSettings, logger)
    {
        _logger = logger;
        KafkaConsumer = new KafkaConsumer(_kafkaSettings, "orders_events_group");
    }

    protected override string TopicName { get; } = "orders_events";
    protected override IKafkaConsumer<long, string> KafkaConsumer { get; }

    protected override async Task HandleAsync(
        IServiceProvider serviceProvider,
        ConsumeResult<long, string> message, 
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _redisOrdersRepository = serviceProvider.GetRequiredService<IRedisOrdersRepository>();

        _logger.LogInformation("Handling messages from Kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);

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

            var updOrder = order with { OrderStatus = From(orderEvent.OrderState) };
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