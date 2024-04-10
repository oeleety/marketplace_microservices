using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Bll;

public class OrderFilterOptions
{
    public IEnumerable<string> ReqRegionsNames { get; set; } = new List<string>();
    public bool FilterOrderType { get; set; } = false;
    public OrderType Type { get; set; }
    public int CustomerId { get; set; } = default;
    public DateTime SinceTimestamp { get; set; } = default;
    public int Limit { get; set; } = -1;
    public int Offset { get; set; } = -1;
    public ValueOrderDal SortColumn { get; set; } = ValueOrderDal.None;
    public bool AscSort { get; set; } = true;
}
