using Ozon.Route256.Practice.LogisticsSimulator.Grpc;

namespace Ozon.Route256.Practice.OrdersService.GrpcClients;

public sealed class LogisticsSimulatorClient
{
    private readonly LogisticsSimulatorService.LogisticsSimulatorServiceClient _client;
    private readonly ILogger<LogisticsSimulatorClient> _logger;

    public LogisticsSimulatorClient(
        LogisticsSimulatorService.LogisticsSimulatorServiceClient client,
        ILogger<LogisticsSimulatorClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<CancelResult> CancelOrderAsync(long id)
    {
        var response = await _client.OrderCancelAsync(new Order { Id = id });
        return response;
    }
}
