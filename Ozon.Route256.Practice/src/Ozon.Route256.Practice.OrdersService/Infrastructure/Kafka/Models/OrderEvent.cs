using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class OrderEvent
{
    public long Id { get; set; }
    public OrderStatusEntity NewState { get; set; }
    public DateTime UpdateDate { get; set; }
}

public enum OrderStatusEntity
{
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled
}
