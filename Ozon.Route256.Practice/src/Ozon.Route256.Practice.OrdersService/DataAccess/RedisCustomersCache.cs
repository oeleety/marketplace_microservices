using System.Text.Json;
using System.Text.Json.Serialization;
using Ozon.Route256.Practice.Proto;
using StackExchange.Redis;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public class RedisCustomersCache
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IDatabase _database;

    public RedisCustomersCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase(0);
    }

    public async Task<bool> IsExistAsync(int orderId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(orderId);

        var contains = await _database.KeyExistsAsync(key);

        return contains;
    }

    public async Task AddAsync(Customer customer, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(customer.Id);

        var resultRedis = JsonSerializer.Serialize(customer, _jsonSerializerOptions);

        await _database.StringSetAsync(key, resultRedis);
    }

    private static RedisKey BuildOrderKey(int orderId)
    {
        return new RedisKey($"customers:{orderId}");
    }
}