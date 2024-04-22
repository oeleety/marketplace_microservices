namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public interface INewOrderProducer
{
    Task ProduceAsync(IReadOnlyCollection<long> validatedPreOrders, CancellationToken token);
}