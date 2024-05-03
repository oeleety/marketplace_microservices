using System.Diagnostics;

namespace Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;

public static class Diagnostics
{
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public const string ActivitySourceName = "OrdersService.Metrics";
    public const string LogisticsServiceCancellation = "LogisticsService Cancellation";
    public const string DbStatusUpdate = "DB status update";
}
