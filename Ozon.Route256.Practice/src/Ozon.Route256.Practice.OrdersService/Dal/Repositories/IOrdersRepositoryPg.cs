using Ozon.Route256.Practice.OrdersService.Bll;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public interface IOrdersRepositoryPg
{
    Task<OrderDal?> FindAsync(long id, CancellationToken token);
    Task<List<(OrderDal order, AddressDal address, RegionDal region)>> GetAllAsync(OrderFilterOptions filterOptions, CancellationToken token);
    Task UpdateStatusAsync(IEnumerable<long> ids, OrderStatus status, CancellationToken token);
}