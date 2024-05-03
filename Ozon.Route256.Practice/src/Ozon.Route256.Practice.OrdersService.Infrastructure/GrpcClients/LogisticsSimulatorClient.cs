using Microsoft.Extensions.Logging;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Proto;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;

public sealed class LogisticsSimulatorClient : ILogisticsSimulatorServiceClient
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

    public async Task<(bool success, string error)> CancelOrderAsync(long id)
    {
        var response = await _client.OrderCancelAsync(new Order { Id = id });
        if(response == null)
        {
            _logger.LogError("order cancel in logistic response is null");
        }
        return (response?.Success == true, response?.Error ?? ""); 
    }
}
