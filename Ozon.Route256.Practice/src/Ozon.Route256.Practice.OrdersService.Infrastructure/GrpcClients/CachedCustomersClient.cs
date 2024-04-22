using Grpc.Core;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;
using Customer = Ozon.Route256.Practice.OrdersService.Infrastructure.Proto.Customer;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;

public class CachedCustomersClient : ICachedCustomersClient
{
    private readonly ICustomersServiceClient _customersService;
    private readonly RedisCustomersCache _customersCache;
    private readonly ILogger<CachedCustomersClient> _logger;

    public CachedCustomersClient(
        ICustomersServiceClient customersService,
        RedisCustomersCache customersCache,
        ILogger<CachedCustomersClient> logger)
    {
        _customersService = customersService;
        _customersCache = customersCache;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Trying to find a customer with id = {id}", id);

        var customer = await _customersCache.Find(id, cancellationToken);
        if (customer == null)
        {
            _logger.LogInformation("Couldn't find a customer with id = {id} in cache. Gonna find in the customer service", id);
            try
            {
                customer = await _customersService.GetCustomerByIdAsync(id, cancellationToken);
                _logger.LogInformation("Found a customer with id = {id} in customerService. Will add to the cache.", id);
                await _customersCache.AddAsync(customer, cancellationToken);
                _logger.LogInformation("Added a customer with id = {id} to cache.", id);

            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
            {
                _logger.LogError("There's no customer with id = {id}", id);
                return null;
            }
        }
        return customer;
    }

    public async Task EnsureExistsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Trying to find a customer with id = {id}", id);

        if (!await _customersCache.IsExistAsync(id, cancellationToken))
        {
            _logger.LogInformation("Couldn't find a customer with id = {id} in cache. Gonna find in the customer service", id);
            var customer = await _customersService.GetCustomerByIdAsync(id, cancellationToken);
            _logger.LogInformation("Found a customer with id = {id} in customerService. Will add to the cache.", id);
            await _customersCache.AddAsync(customer, cancellationToken);
            _logger.LogInformation("Added a customer with id = {id} to cache.", id);
        }
    }
}
