﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public class ShardConnectionFactory : IShardPostgresConnectionFactory
{
    private readonly IDbStore _dbStore;
    private readonly DbOptions _dbOptions;
    private readonly ILogger<ShardConnectionFactory> _logger;
    private Dictionary<int, NpgsqlDataSource> _dataSourceByBucket = new();

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

        var dataSource = GetMappedDataSource(connectionString, bucketId);
        var connection = await dataSource.OpenConnectionAsync(token);
        return new ShardNpgsqlConnection(connection, bucketId);
    } 

    private NpgsqlDataSource GetMappedDataSource(string connection, int bucketId)
    {
        if(_dataSourceByBucket.TryGetValue(bucketId, out var dataSource))
        {
            return dataSource;
        }
        else
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connection);
            dataSourceBuilder.MapEnum<OrderTypeDal>("order_type");
            dataSourceBuilder.MapEnum<OrderStatusDal>("order_status");
            dataSourceBuilder.MapComposite<AddressDalToInsert>("address_dal_to_insert");

            var dataSourceNew = dataSourceBuilder.Build();
            _dataSourceByBucket.Add(bucketId, dataSourceNew);
            return dataSourceNew;
        }
    }
}