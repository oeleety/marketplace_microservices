namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

public sealed class PreOrder
{
    public long Id { get; set; }
    public OrderSource Source { get; set; }
    public Customer Customer { get; set; }
    public List<Good> Goods { get; set; }
}

public sealed class Customer
{
    public int Id { get; set; }
    public Address Address { get; set; }
}

public sealed class Good
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public long Weight { get; set; }
}

public sealed class Address
{
    public string Region { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Building { get; set; }
    public string Apartment { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public enum OrderSource
{
    WebSite = 1,
    Mobile = 2,
    Api = 3
}