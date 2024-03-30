
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection)
    {
        collection.AddSingleton<IKafkaDataProvider<long, string>, OrderDataProvider>();
        collection.AddHostedService<PreOrderConsumer>();

        return collection;
    }
}