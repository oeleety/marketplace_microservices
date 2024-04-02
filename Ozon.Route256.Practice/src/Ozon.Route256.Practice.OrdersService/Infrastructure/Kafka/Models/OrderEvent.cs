namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class OrderEvent
{
    public long Id { get; set; }
    public OrderStatusEntity OrderState { get; set; }
    public DateTime ChangedAt { get; set; }
}

public enum OrderStatusEntity
{
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled
}
