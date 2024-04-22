using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;
using GrpcReplicaType = Ozon.Route256.Practice.Replica.Types.ReplicaType;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public interface IShardMigrator
{
    Task MigrateAsync(CancellationToken token);
}

public class ShardMigrator : IShardMigrator
{
    private readonly DbOptions _dbOptions;
    private readonly SdService.SdServiceClient _client;
    private readonly ILogger<ShardMigrator> _logger;

    public ShardMigrator(
        ILogger<ShardMigrator> logger,
        IOptions<DbOptions> dbOptions,
        SdService.SdServiceClient client)
    {
        _dbOptions   = dbOptions.Value;
        _client = client;
        _logger = logger;
    }

    public async Task MigrateAsync(
        CancellationToken token)
    {
        var endpoints = await GetEndpointsAsync(token);

        foreach (var endpoint in endpoints)
        {
            var connectionString = endpoint.GetConnectionString(_dbOptions);
            foreach (var bucketId in endpoint.Buckets)
            {
                var serviceProvider = CreateServices(connectionString);
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BucketMigrationContext>();
                context.UpdateCurrentDbSchema(bucketId);
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                runner.MigrateUp();
            }
        }
    }

    private static IServiceProvider CreateServices(
        string connectionString)
    {
        return new ServiceCollection()
            .AddSingleton<BucketMigrationContext>()
            .AddFluentMigratorCore()
            .AddLogging(o => o.AddFluentMigratorConsole())
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .WithMigrationsIn(typeof(ShardSqlMigration).Assembly)
                .ScanIn(typeof(ShardVersionTableMetaData).Assembly).For.VersionTableMetaData()
            )
            .BuildServiceProvider();
    }
    
    private async Task<DbEndpoint[]> GetEndpointsAsync(CancellationToken token)
    {
        var request = new DbResourcesRequest
        {
            ClusterName = _dbOptions.ClusterName
        };
        await Task.Delay(10000, token);

        using var stream = 
            _client.DbResources(request, cancellationToken: token);

        await stream.ResponseStream.MoveNext(token);
        var response = stream.ResponseStream.Current;
        return GetEndpoints(response).ToArray();
    }
    
    private static IEnumerable<DbEndpoint> GetEndpoints(DbResourcesResponse response) =>
        response.Replicas.Select(replica => new DbEndpoint(
            $"{replica.Host}:{replica.Port}",
            ToDbReplica(replica.Type),
            replica.Buckets.ToArray()));
    
    private static DbReplicaType ToDbReplica(GrpcReplicaType replicaType) =>
        replicaType switch
        {
            GrpcReplicaType.Master => DbReplicaType.Master,
            GrpcReplicaType.Async  => DbReplicaType.Async,
            GrpcReplicaType.Sync   => DbReplicaType.Sync,
            _ => throw new ArgumentOutOfRangeException(nameof(replicaType), replicaType, null)
        };
}