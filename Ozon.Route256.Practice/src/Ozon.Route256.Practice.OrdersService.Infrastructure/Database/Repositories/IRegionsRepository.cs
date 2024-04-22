
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Repositories;

public interface IRegionsRepository
{
    Task<RegionDal[]> FindManyAsync(IEnumerable<string> names, CancellationToken token);
    Task<RegionDal[]> GetAllAsync(CancellationToken token);
}