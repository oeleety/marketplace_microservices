using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public interface IOrdersAggregateRepository
{
    Task AddAsync(OrderAggregate order, CancellationToken token);
    Task UpdateOrderStatusAsync(long id, OrderStatusModel status, CancellationToken token);
    Task<Region[]> GetRegionsAsync(CancellationToken token);
    Task<IEnumerable<OrderAggregate>> GetOrdersAsync(OrderFilterOptions filterOptions, CancellationToken token, bool internalRequest = false);
    Task<IEnumerable<Region>> FindRegionsAsync(IEnumerable<string> reqRegionsNames, CancellationToken token);
    Task<Order> FindOrderAsync(long id, CancellationToken token, bool internalRequest = false);

}