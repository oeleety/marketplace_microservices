namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class NewOrder
{
    public long Id { get; set; }
    public int Source { get; set; }
    public int CustomerId { get; set; }
    public string CustomerFullName { get; set; }
    public string CustomerMobileNumber { get; set; }
    public Address DeliveryAddress { get; set; }
    public List<Good> Goods { get; set; }
}