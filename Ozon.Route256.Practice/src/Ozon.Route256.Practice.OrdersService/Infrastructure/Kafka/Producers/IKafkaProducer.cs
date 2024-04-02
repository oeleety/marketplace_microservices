using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

public interface IKafkaProducer<TKey, TValue>
{
    IProducer<TKey, TValue> Producer { get; }
}