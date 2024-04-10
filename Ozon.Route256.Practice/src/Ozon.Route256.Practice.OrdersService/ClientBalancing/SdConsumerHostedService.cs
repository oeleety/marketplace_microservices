using Grpc.Core;
using GrpcReplicaType = Ozon.Route256.Practice.Replica.Types.ReplicaType;

namespace Ozon.Route256.Practice.OrdersService.ClientBalancing;

public sealed class SdConsumerHostedService : BackgroundService
{
    private const int SdTimeToDelayMs = 1000;

    private readonly SdService.SdServiceClient _client;
    private readonly ILogger<SdConsumerHostedService> _logger;
    private readonly IDbStore _dbStore;

    public SdConsumerHostedService(
        IDbStore dbStore,
        SdService.SdServiceClient client,
        ILogger<SdConsumerHostedService> logger)
    {
        _dbStore = dbStore;
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var request = new DbResourcesRequest
                {
                    ClusterName = "cluster"
                };

                using var stream =
                    _client.DbResources(
                        request,
                        cancellationToken: cancellationToken);

                await foreach (var response in stream.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    _logger.LogInformation(
                        "Get a new data from SD. Timestamp {Timestamp}",
                        response.LastUpdated.ToDateTime());
                    
                    var endpoints = GetEndpoints(response).ToList();

                    await _dbStore.UpdateEndpointAsync(endpoints);
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "SD throw exception.");
                await Task.Delay(SdTimeToDelayMs, cancellationToken);
            }
        }
    }

    private static IEnumerable<DbEndpoint> GetEndpoints(DbResourcesResponse response) =>
        response.Replicas.Select(replica => new DbEndpoint(
            $"{replica.Host}:{replica.Port}",
            ToDbReplica(replica.Type),
            replica.Buckets.ToArray()
        ));

    private static DbReplicaType ToDbReplica(GrpcReplicaType replicaType) =>
        replicaType switch
        {
            GrpcReplicaType.Master => DbReplicaType.Master,
            GrpcReplicaType.Sync => DbReplicaType.Sync,
            GrpcReplicaType.Async => DbReplicaType.Async,
            _ => throw new ArgumentOutOfRangeException(nameof(replicaType), replicaType, null)
        };
}
