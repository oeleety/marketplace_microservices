namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public interface ILogisticsSimulatorServiceClient
{
    Task<(bool success, string error)> CancelOrderAsync(long id);
}