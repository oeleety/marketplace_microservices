namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public interface IShardPostgresConnectionFactory
{
    IEnumerable<int> GetAllBuckets();
    Task<ShardNpgsqlConnection> OpenConnectionByBucketIdAsync(int bucketId, CancellationToken token);
}
