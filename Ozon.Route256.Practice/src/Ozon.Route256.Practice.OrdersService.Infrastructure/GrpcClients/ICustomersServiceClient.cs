using Ozon.Route256.Practice.OrdersService.Infrastructure.Proto;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;

public interface ICustomersServiceClient
{
    Task CreateCustomerAsync(Customer customer, CancellationToken cancellationToken);
    Task<Customer> GetCustomerByIdAsync(int id, CancellationToken cancellationToken);
}