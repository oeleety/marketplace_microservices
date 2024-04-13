using Ozon.Route256.Practice.OrdersService.Bll;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public interface IOrdersRepositoryPg
{
    Task CreateAsync(OrderDal order, CancellationToken token);
    Task<OrderDal?> FindAsync(long id, CancellationToken token, bool internalRequest = false);
    Task<List<(OrderDal order, AddressDal address, RegionDal region)>> GetAllAsync(OrderFilterOptions filterOptions, CancellationToken token, bool internalRequest = false);
    Task UpdateStatusAsync(long id, OrderStatus status, CancellationToken token);
}