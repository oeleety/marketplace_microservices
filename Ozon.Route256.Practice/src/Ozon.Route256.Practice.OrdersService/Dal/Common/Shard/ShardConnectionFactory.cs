using Microsoft.Extensions.Options;
using Npgsql;
using Ozon.Route256.Practice.OrdersService.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;

public interface IShardPostgresConnectionFactory
{
    IEnumerable<int> GetAllBuckets();
    Task<ShardNpgsqlConnection> OpenConnectionByBucketIdAsync(int bucketId, CancellationToken token);
}

public class ShardConnectionFactory: IShardPostgresConnectionFactory
{
    private readonly IDbStore _dbStore;
    private readonly DbOptions _dbOptions;
    private readonly ILogger<ShardConnectionFactory> _logger;

    public ShardConnectionFactory(
        ILogger<ShardConnectionFactory> logger,
        IDbStore dbStore,
        IOptions<DbOptions> dbOptions)
    {
        _logger = logger;
        _dbStore   = dbStore;
        _dbOptions = dbOptions.Value;
    }

    public IEnumerable<int> GetAllBuckets()
    {
        for (int bucketId = 0; bucketId < _dbStore.BucketsCount; bucketId++)
        {
            yield return bucketId;
        }
    }

    public async Task<ShardNpgsqlConnection> OpenConnectionByBucketIdAsync(
        int bucketId, 
        CancellationToken token)
    {
        var endpoint = _dbStore.GetEndpointByBucket(bucketId);
        var connectionString = endpoint.GetConnectionString(_dbOptions);

        var dataSource = GetMappedDataSource(connectionString);
        var connection = await dataSource.OpenConnectionAsync(token);
        return new ShardNpgsqlConnection(connection, bucketId);
    } 

    private static NpgsqlDataSource GetMappedDataSource(string connection)
    {
        //todo https://github.com/npgsql/npgsql/issues/5064
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connection);
        dataSourceBuilder.MapEnum<OrderType>("order_type");
        dataSourceBuilder.MapEnum<OrderStatus>("order_status");
        dataSourceBuilder.MapComposite<AddressDalToInsert>("address_dal_to_insert");

        return dataSourceBuilder.Build();
    }
}