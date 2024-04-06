using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.Bll;

public interface IOrdersService
{
    Task CancelOrderAsync(long id, CancellationToken token);
    Task<List<OrdersStatisticEntity>> GetAggregatedOrdersByRegionAsync(RegionEntity[] reqRegions, DateTime sinceTimestamp, CancellationToken token);
    Task<OrderEntity[]> GetOrdersAsync(RegionEntity[] reqRegions, OrderTypeEntity orderType, PaginationEntity pagination, CancellationToken token, SortOrderEntity sortOrder = SortOrderEntity.Asc, ValueOrderEntity valueOrder = ValueOrderEntity.None);
    Task<IReadOnlyCollection<OrderEntity>> GetOrdersByCustomerAsync(int customerId, DateTime sinceTimestamp, PaginationEntity pagination, CancellationToken token);
    Task<OrderStatusEntity> GetOrderStatusAsync(long id, CancellationToken token);
    Task<RegionEntity[]> GetRegionsAsync(CancellationToken token);
}