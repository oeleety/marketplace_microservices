using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Repositories;

public interface IAddressesRepository
{
    Task<int[]> CreateAsync(AddressDalToInsert[] addresses, CancellationToken token);
}