using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public interface IRedisOrdersRepository
{
    Task AddOrderAsync(OrderEntity order, CancellationToken token);
    Task CancelOrderAsync(long id, CancellationToken token);
    Task<OrderEntity?> FindAsync(long orderId, CancellationToken token);
    Task<OrderStatusEntity> GetOrderStatusAsync(long id, CancellationToken token, bool filterPreorders = true);
    Task<bool> IsExistAsync(long id, CancellationToken token);
    Task UpdateAsync(OrderEntity order, CancellationToken token);
}