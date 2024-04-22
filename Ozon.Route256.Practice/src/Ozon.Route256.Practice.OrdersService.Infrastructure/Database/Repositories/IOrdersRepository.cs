using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Repositories;

public interface IOrdersRepository
{
    Task CreateAsync(OrderDal order, CancellationToken token);
    Task<OrderDal?> FindAsync(long id, CancellationToken token, bool internalRequest = false);
    Task<List<(OrderDal order, AddressDal address, RegionDal region)>> GetAllAsync(OrderFilterOptions filterOptions, CancellationToken token, bool internalRequest = false);
    Task UpdateStatusAsync(long id, OrderStatusDal status, CancellationToken token);
}