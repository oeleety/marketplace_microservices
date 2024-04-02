using Confluent.Kafka;
using Ozon.Route256.Practice.OrdersService.Configuration;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

internal sealed class KafkaProducer : IKafkaProducer<long, string>
{
    public KafkaProducer(
        ILogger<KafkaProducer> logger,
        IOptions<KafkaSettings> settings)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = settings.Value.Servers,
        };

        ProducerBuilder<long, string> producerBuilder = new(producerConfig);

        Producer = producerBuilder
            .SetErrorHandler((_, error) => { logger.LogError(error.Reason); })
            .SetLogHandler((_, message) => logger.LogInformation(message.Message))
            .Build();
    }

    public IProducer<long, string> Producer { get; }

    public void Dispose()
    {
        Producer.Flush(TimeSpan.FromSeconds(10));
        Producer.Dispose();
    }
}