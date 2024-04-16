using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public interface IRegionsRepositoryPg
{
    Task<RegionDal[]> FindManyAsync(IEnumerable<string> names, CancellationToken token);
    Task<RegionDal[]> GetAllAsync(CancellationToken token);
}