using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public class OrderFilterOptions
{
    public IEnumerable<string> ReqRegionsNames { get; set; } = Array.Empty<string>();
    public bool FilterOrderType { get; set; } = false;
    public OrderTypeModel OrderType { get; set; }
    public int CustomerId { get; set; } = default;
    public DateTime SinceTimestamp { get; set; } = default;
}
