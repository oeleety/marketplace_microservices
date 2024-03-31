using System.Text.Json.Serialization;
using System.Text.Json;
using StackExchange.Redis;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;
using Ozon.Route256.Practice.OrdersService.Exceptions;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public sealed class RedisOrdersRepository : IRedisOrdersRepository
{
    private static readonly HashSet<OrderStatusEntity> _forbiddenToCancelStatus = new()
    {
        OrderStatusEntity.Cancelled,
        OrderStatusEntity.Delivered
    };

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

    public async Task AddOrderAsync(OrderEntity order, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(order.Id);

        var resultRedis = JsonSerializer.Serialize(order, _jsonSerializerOptions);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task<bool> IsExistAsync(long id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(id);

        var contains = await _database.KeyExistsAsync(key);

        return contains;
    }

    public async Task UpdateAsync(OrderEntity order, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(order.Id);

        if (!_database.KeyExists(key))
        {
            throw new Exception($"Order with id {order.Id} not found");
        }

        var resultRedis = JsonSerializer.Serialize(order, _jsonSerializerOptions);

        await _database.StringSetAsync(key, resultRedis);
    }

    public async Task<OrderEntity?> FindAsync(long orderId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = BuildOrderKey(orderId);

        var resultRedis = await _database.StringGetAsync(key);

        var result = resultRedis.HasValue
            ? JsonSerializer.Deserialize<OrderEntity>(resultRedis.ToString(), _jsonSerializerOptions)
            : null;

        return result;
    }

    public async Task CancelOrderAsync(long id, CancellationToken token)
    {
        var order = await ThrowIfCancelProhibitedAsync(id, token);
        var updatedOrder = order with { OrderStatus = OrderStatusEntity.Cancelled };
        token.ThrowIfCancellationRequested();
        await UpdateAsync(updatedOrder, token);
    }

    public async Task<OrderStatusEntity> GetOrderStatusAsync(
        long id,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindAsync(id, token);
        if (order is null)
        {
            throw new NotFoundException($"Order with id={id} not found");
        }
        return order.OrderStatus;
    }

    private static RedisKey BuildOrderKey(long orderId)
    {
        return new RedisKey($"orders:{orderId}");
    }

    private async Task<OrderEntity> ThrowIfCancelProhibitedAsync(long id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindAsync(id, token);
        if (_forbiddenToCancelStatus.Contains(order.OrderStatus))
        {
            throw new UnprocessableException($"Cannot cancel order with id={id} in state {order.OrderStatus}.");
        }
        return order;
    }
}
