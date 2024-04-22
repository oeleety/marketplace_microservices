using Microsoft.Extensions.Logging;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Proto;
using Customer = Ozon.Route256.Practice.OrdersService.Infrastructure.Proto.Customer;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;

public sealed class CustomersServiceClient : ICustomersServiceClient
{
    private readonly Customers.CustomersClient _client;
    private readonly ILogger<CustomersServiceClient> _logger;

    public CustomersServiceClient(
       Customers.CustomersClient client,
        ILogger<CustomersServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Customer> GetCustomerByIdAsync(int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trying to find a customer with id = {id}..", id);
        var response = await _client.GetCustomerByIdAsync(
            new GetCustomerByIdRequest { Id = id },
            cancellationToken: cancellationToken);
        _logger.LogInformation("Found a customer with id = {id}", id);

        return response.Customer;
    }

    public async Task CreateCustomerAsync(Customer customer,
    CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trying to create a customer with id = {customer.Id}..", customer.Id);
        await _client.CreateCustomerAsync(
            new CreateCustomerRequest { Customer = customer },
            cancellationToken: cancellationToken);
        _logger.LogInformation("Created a customer with id = {customer.Id}..", customer.Id);

        return;
    }
}
