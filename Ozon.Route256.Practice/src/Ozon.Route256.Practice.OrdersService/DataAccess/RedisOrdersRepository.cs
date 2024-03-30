using System.Text.Json.Serialization;
using System.Text.Json;
using StackExchange.Redis;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public sealed class RedisOrdersRepository //: IOrdersRepository todo?
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IDatabase _database;

    public RedisOrdersRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase(0);
    }

    public async Task AddOrderAsync(NewOrder order, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(order.Id);

        var resultRedis = JsonSerializer.Serialize(order, _jsonSerializerOptions);

        await _database.StringSetAsync(key, resultRedis);
    }
    
    private static RedisKey BuildOrderKey(long orderId)
    {
        return new RedisKey($"orders:{orderId}");
    }
}
