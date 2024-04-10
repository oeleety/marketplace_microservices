using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.Bll
{
    public interface IOrdersService
    {
        Task AddOrderAsync(OrderEntity order, CancellationToken token);
        Task CancelOrderAsync(long id, CancellationToken token, bool internalRequest = false);
        Task<OrderEntityBase> FindOrderAsync(long id, CancellationToken token, bool internalRequest = false);
        Task<List<OrdersStatisticEntity>> GetAggregatedOrdersByRegionAsync(RegionEntity[] reqRegions, DateTime sinceTimestamp, CancellationToken token, bool internalRequest = false);
        Task<OrderEntity[]> GetOrdersAsync(RegionEntity[] reqRegions, OrderTypeEntity orderType, PaginationEntity pagination, CancellationToken token, SortOrderEntity sortOrder = SortOrderEntity.Asc, ValueOrderEntity valueOrder = ValueOrderEntity.None, bool internalRequest = false);
        Task<IReadOnlyCollection<OrderEntity>> GetOrdersByCustomerAsync(int customerId, DateTime sinceTimestamp, PaginationEntity pagination, CancellationToken token, bool internalRequest = false);
        Task<OrderStatusEntity> GetOrderStatusAsync(long id, CancellationToken token, bool internalRequest = false);
        Task<RegionEntity[]> GetRegionsAsync(CancellationToken token);
        Task<bool> IsOrderExistAsync(long id, CancellationToken token, bool internalRequest = false);
        Task UpdateOrderStatusAsync(long id, OrderStatusEntity status, CancellationToken token);
    }
}