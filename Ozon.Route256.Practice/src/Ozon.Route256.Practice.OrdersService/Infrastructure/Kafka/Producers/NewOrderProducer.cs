using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

internal class NewOrderProducer : INewOrderProducer
{
    private const string TopicName = "new_orders";

    private readonly IKafkaDataProvider<long, string> _kafkaDataProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    private readonly ILogger<NewOrderProducer> _logger;

    public NewOrderProducer(
        IKafkaDataProvider<long, string> kafkaDataProvider,
        ILogger<NewOrderProducer> logger)
    {
        _logger = logger;
        _kafkaDataProvider = kafkaDataProvider;
    }

    public async Task ProduceAsync(IReadOnlyCollection<long> validatedPreOrders, CancellationToken token)
    {
        await Task.Yield();

        var tasks = new List<Task<DeliveryResult<long, string>>>(validatedPreOrders.Count);
        _logger.LogInformation("Started producing into topic {TopicName}", TopicName);

        foreach (var orderId in validatedPreOrders)
        {
            token.ThrowIfCancellationRequested();

            var key = orderId;
            var value = JsonSerializer.Serialize(orderId, _jsonSerializerOptions);

            var message = new Message<long, string>
            {
                Key = key,
                Value = value
            };
            var task = _kafkaDataProvider.Producer.ProduceAsync(TopicName, message, token);
            tasks.Add(task);
        }
        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Produced succesfully into topic {TopicName}", TopicName);

        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Got exception whule producing into topic {TopicName}", TopicName);
        }
    }
}