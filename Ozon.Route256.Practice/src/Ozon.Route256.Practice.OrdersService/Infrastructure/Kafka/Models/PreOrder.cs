namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class PreOrder
{
    public long Id { get; set; }
    public int Source { get; set; }
    public Customer Customer { get; set; }
    public List<Good> Goods { get; set; }
}

public sealed class Customer
{
    public int Id { get; set; }
    public Address Address { get; set; }
}