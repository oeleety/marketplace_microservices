using Confluent.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Consumers;

public interface IKafkaConsumer<TKey, TValue>
{
    IConsumer<TKey, TValue> Consumer { get; }
}