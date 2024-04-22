using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumer(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection.AddHostedService<PreOrderConsumerService>();
        collection.AddHostedService<OrdersEventsConsumerService>();
        collection.Configure<KafkaConsumerSettings>(o =>
        {
            o.Servers = configuration.TryGetValue("ROUTE256_KAFKA_BROKERS");
        });

        return collection;
    }
}