using Ozon.Route256.Practice.OrdersService.Application.Helpers;
using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public interface IOrdersService
{
    Task AddOrderAsync(OrderAggregate order, CancellationToken token);
    Task CancelOrderAsync(long id, CancellationToken token, bool internalRequest = false);
    Task<Order> FindOrderAsync(long id, CancellationToken token, bool internalRequest = false);
    Task<List<OrdersStatisticEntity>> GetAggregatedOrdersByRegionAsync(Region[] reqRegions, DateTime sinceTimestamp, CancellationToken token, bool internalRequest = false);
    Task<OrderAggregate[]> GetOrdersAsync(Region[] reqRegions, OrderTypeModel orderType, Pagination pagination, CancellationToken token, SortOrderDto sortOrder = SortOrderDto.Asc, ValueOrderDto valueOrder = ValueOrderDto.None, bool internalRequest = false);
    Task<IReadOnlyCollection<OrderAggregate>> GetOrdersByCustomerAsync(int customerId, DateTime sinceTimestamp, Pagination pagination, CancellationToken token, bool internalRequest = false);
    Task<OrderStatusModel> GetOrderStatusAsync(long id, CancellationToken token, bool internalRequest = false);
    Task<Region[]> GetRegionsAsync(CancellationToken token);
    Task<bool> IsOrderExistAsync(long id, CancellationToken token, bool internalRequest = false);
    Task HandleNewStatusAsync(long id, OrderStatusModel status, CancellationToken token);
}