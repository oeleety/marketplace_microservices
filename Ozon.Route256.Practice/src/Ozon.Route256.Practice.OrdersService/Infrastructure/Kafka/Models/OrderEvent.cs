using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class OrderEvent
{
    public long OrderId { get; set; }
    public OrderStatusModel OrderState { get; set; }
    public DateTime ChangedAt { get; set; }
}
