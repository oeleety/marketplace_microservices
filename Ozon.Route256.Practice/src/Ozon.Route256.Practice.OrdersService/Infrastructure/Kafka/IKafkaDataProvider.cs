using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public interface IKafkaDataProvider<TKey, TValue>
{
    IConsumer<TKey, TValue> Consumer { get; }

    IProducer<TKey, TValue> Producer { get; }
}