using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public interface IOrdersRepository
{
    Task CancelOrderAsync(long id, CancellationToken token = default);

    Task<OrderStatusEntity> GetOrderStatusAsync(long id, CancellationToken token = default);

    Task<RegionEntity[]> GetRegions(CancellationToken token = default);

    Task<OrderEntity[]> GetOrders(
        RegionEntity[] regions, 
        OrderTypeEntity orderType, 
        PaginationEntity pagination, 
        SortOrderEntity sortOrder = SortOrderEntity.Asc,
        ValueOrderEntity valueOrder = ValueOrderEntity.None,
        CancellationToken token = default);

    Task<IReadOnlyCollection<OrderEntity>> GetOrdersByCustomer(
        long customerId, 
        DateTime sinceTimestamp, 
        PaginationEntity pagination, 
        CancellationToken token = default);

    Task<Dictionary<int, OrdersStatisticEntity>> GetAggregatedOrdersByRegion(
        RegionEntity[] regions, 
        DateTime sinceTimestamp, 
        CancellationToken token = default);
}