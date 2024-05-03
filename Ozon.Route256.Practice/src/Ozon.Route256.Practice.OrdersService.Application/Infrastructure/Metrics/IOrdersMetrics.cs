using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;

public interface IOrdersMetrics
{
    void CancellationTried(bool success);
    void OrderCancelled(OrderStatusModel status);
}
