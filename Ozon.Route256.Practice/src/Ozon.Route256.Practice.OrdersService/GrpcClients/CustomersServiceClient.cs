using Ozon.Route256.Practice.Proto;

namespace Ozon.Route256.Practice.OrdersService.GrpcClients;

public sealed class CustomersServiceClient
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
        _logger.LogInformation("Trying to find a customer with id = {id}", id);
        var response = await _client.GetCustomerByIdAsync(
            new GetCustomerByIdRequest { Id = id },
            cancellationToken: cancellationToken);
        _logger.LogInformation("Found a customer with id = {id}", id);

        return response.Customer;
    }
}
