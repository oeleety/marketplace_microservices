using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection collection)
    {
        collection.AddSingleton<IKafkaProducer<long, string>, KafkaProducer>();
        collection.AddHostedService<PreOrderConsumerService>();
        collection.AddHostedService<OrdersEventsConsumerService>();
        collection.AddSingleton<INewOrderProducer, NewOrderProducerService>();

        return collection;
    }
}