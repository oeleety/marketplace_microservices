namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public class BaseShardRepository
{
    private readonly IShardPostgresConnectionFactory _connectionFactory;
    private readonly IShardingRule<long> _longShardingRule;

    public BaseShardRepository(
        IShardPostgresConnectionFactory connectionFactory,
        IShardingRule<long> longShardingRule)
    {
        _connectionFactory  = connectionFactory;
        _longShardingRule = longShardingRule;
    }

    protected async Task<ShardNpgsqlConnection> OpenConnectionByShardKeyAsync(
        long shardKey, CancellationToken token)
    {
        var bucketId = _longShardingRule.GetBucketId(shardKey);
        return await _connectionFactory.OpenConnectionByBucketIdAsync(bucketId, token);
    }

    protected async Task<ShardNpgsqlConnection> OpenConnectionByBucket(
        int bucketId,
        CancellationToken token)
    {
        return await _connectionFactory.OpenConnectionByBucketIdAsync(bucketId, token);
    }

    protected int GetBucketByShardKey(long shardKey) => 
        _longShardingRule.GetBucketId(shardKey);
    
    protected IEnumerable<int> AllBuckets => _connectionFactory.GetAllBuckets();
}