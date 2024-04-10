using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class OrderEvent
{
    public long OrderId { get; set; }
    public OrderStatusEntity OrderState { get; set; }
    public DateTime ChangedAt { get; set; }
}
