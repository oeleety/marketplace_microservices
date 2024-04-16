using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public interface IAddressesRepositoryPg
{
    Task<int[]> CreateAsync(AddressDalToInsert[] addresses, CancellationToken token);
}