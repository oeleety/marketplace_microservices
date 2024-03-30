
namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Producers;

public interface INewOrderProducer
{
    Task ProduceAsync(IReadOnlyCollection<long> validatedPreOrders, CancellationToken token);
}