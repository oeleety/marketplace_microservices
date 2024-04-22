using System.Text.Json.Serialization;
using System.Text.Json;
using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Application.Bll;

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
    private IOrdersService _ordersService;

    public OrdersEventsConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaConsumerSettings> kafkaSettings,
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
        _ordersService = serviceProvider.GetRequiredService<IOrdersService>();

        _logger.LogInformation("Handling messages from Kafka {TopicName} {message.Message.Value}", TopicName, message.Message.Value);

        var value = message.Message.Value;
        var orderEvent= JsonSerializer.Deserialize<OrderEvent>(value, _jsonSerializerOptions);

        var isExist = await _ordersService.IsOrderExistAsync(orderEvent.OrderId, cancellationToken, internalRequest: true);
        if (!isExist)
        {
            _logger.LogError("No orders with id={orderEvent.Id} is found in repository. Couldn't handle event.", orderEvent.OrderId);
        }
        else
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _ordersService.HandleNewStatusAsync(orderEvent.OrderId, orderEvent.OrderState, cancellationToken);
            _logger.LogInformation("Order id = {orderEvent.Id} status updated", orderEvent.OrderId);
        }
    }
}