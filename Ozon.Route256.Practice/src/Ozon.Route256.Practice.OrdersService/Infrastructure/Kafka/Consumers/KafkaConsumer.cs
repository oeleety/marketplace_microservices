using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Configuration;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

internal sealed class KafkaConsumer : IKafkaConsumer<long, string>
{
    public KafkaConsumer(
        IOptions<KafkaSettings> settings,
        string groupId)
    {
        var factory = LoggerFactory.Create(builder => {
            builder.AddConsole();
        });

        var logger = factory.CreateLogger<KafkaConsumer>();
        var consumerConfig = new ConsumerConfig
        {
            GroupId = groupId,
            BootstrapServers = settings.Value.Servers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        ConsumerBuilder<long, string> consumerBuilder = new(consumerConfig);
        Consumer = consumerBuilder
            .SetErrorHandler((_, error) => { logger.LogError(error.Reason); })
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();
    }

    public IConsumer<long, string> Consumer { get; }
}