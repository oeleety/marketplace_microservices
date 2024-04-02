using System.Text.Json;
using System.Text.Json.Serialization;
using Ozon.Route256.Practice.Proto;
using StackExchange.Redis;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public sealed class RedisCustomersCache
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IDatabase _database;
    private readonly ILogger<RedisCustomersCache> _logger;

    public RedisCustomersCache(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCustomersCache> logger)
    {
        _database = connectionMultiplexer.GetDatabase(0);
        _logger = logger;
    }

    public async Task<Customer?> Find(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildCustomerKey(id);
        
        _logger.LogInformation("Trying to find a customer with id = {id}", id);
        var resultRedis = await _database.StringGetAsync(key);
        _logger.LogInformation(" Result of searching a customer with id = {id} is {result.HasValue}", id, resultRedis.HasValue);

        var result = resultRedis.HasValue 
            ? JsonSerializer.Deserialize<Customer>(resultRedis.ToString(), _jsonSerializerOptions) 
            : null;

        return result;
    }

    public async Task<bool> IsExistAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildCustomerKey(id);
        
        _logger.LogInformation("Trying to find a customer with id = {id}", id);
        var contains = await _database.KeyExistsAsync(key);
        _logger.LogInformation(" Result of searching a customer with id = {id} is {contains}", id, contains);

        return contains;
    }

    public async Task AddAsync(Customer customer, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildCustomerKey(customer.Id);

        var resultRedis = JsonSerializer.Serialize(customer, _jsonSerializerOptions);

        await _database.StringSetAsync(key, resultRedis);
    }

    private static RedisKey BuildCustomerKey(int id)
    {
        return new RedisKey($"customers:{id}");
    }
}