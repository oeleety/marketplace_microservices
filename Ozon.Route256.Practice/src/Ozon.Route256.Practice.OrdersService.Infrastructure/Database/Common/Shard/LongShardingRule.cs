using Murmur;
using Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public interface IShardingRule<TShardKey>
{
    int GetBucketId(TShardKey shardKey);
}

public class LongShardingRule : IShardingRule<long>, IDisposable
{
    private readonly IDbStore _dbStore;
    private readonly Murmur32 _hashAlgorithm;

    public LongShardingRule(
        IDbStore dbStore)
    {
        _dbStore = dbStore;
        _hashAlgorithm = MurmurHash.Create32();
    }

    public int GetBucketId(
        long shardKey)
    {
        var shardKeyHashCode = GetShardKeyHashCode(shardKey);

        return Math.Abs(shardKeyHashCode) % _dbStore.BucketsCount;
    }

    private int GetShardKeyHashCode(
        long shardKey)
    {
        var bytes = BitConverter.GetBytes(shardKey);
        var hash = _hashAlgorithm.ComputeHash(bytes);
        return BitConverter.ToInt32(hash);
    }

    public void Dispose()
    {
        _hashAlgorithm.Dispose();
    }
}