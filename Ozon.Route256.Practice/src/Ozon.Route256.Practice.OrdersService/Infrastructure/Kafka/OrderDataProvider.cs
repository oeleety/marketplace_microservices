using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.Configuration;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

internal sealed class OrderDataProvider : IKafkaDataProvider<long, string>
{
    public OrderDataProvider(
        ILogger<OrderDataProvider> logger,
        IOptions<KafkaSettings> settings)
    {
        var consumerConfig = new ConsumerConfig
        {
            GroupId = "pre_order_group",
            BootstrapServers = settings.Value.Servers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        ConsumerBuilder<long, string> consumerBuilder = new(consumerConfig);
        Consumer = consumerBuilder
            .SetErrorHandler((_, error) => { logger.LogError(error.Reason); })
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:29091",
        };

        ProducerBuilder<long, string> producerBuilder = new(producerConfig);

        Producer = producerBuilder
            .SetErrorHandler((_, error) => { logger.LogError(error.Reason); })
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();
    }

    public IConsumer<long, string> Consumer { get; }

    public IProducer<long, string> Producer { get; }
}