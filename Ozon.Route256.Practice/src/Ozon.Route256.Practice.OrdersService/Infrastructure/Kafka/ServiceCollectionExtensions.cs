using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection)
    {
        collection.AddSingleton<IKafkaDataProvider<long, string>, OrderDataProvider>();
        collection.AddHostedService<PreOrderConsumer>();
        collection.AddSingleton<INewOrderProducer, NewOrderProducer>();

        return collection;
    }
}