using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

internal sealed class KafkaProducer : IKafkaProducer<long, string>
{
    public KafkaProducer(
        ILogger<KafkaProducer> logger,
        IOptions<KafkaProducerSettings> settings)
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