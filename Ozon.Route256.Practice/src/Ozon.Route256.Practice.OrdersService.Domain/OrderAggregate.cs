namespace Ozon.Route256.Practice.OrdersService.Domain;

public sealed class OrderAggregate
{
    public readonly Order Order;
    public readonly Address Address;
    public readonly Region CreatedRegion;

    public OrderAggregate(
        Order order,
        Address address,
        Region createdRegion)
    {
        Order = order;
        Address = address;
        CreatedRegion = createdRegion;
    }

    public OrderAggregate(
        long id,
        OrderStatusModel orderStatus,
        OrderTypeModel orderType,
        int customerId,
        string customerFullName,
        string customerMobileNumber,
        Address deliveryAddress,
        int itemsCount,
        decimal price,
        double weight,
        DateTime created,
        Region createdRegion)
    {
        Order = new(id, orderStatus, orderType, customerId, customerFullName, customerMobileNumber, itemsCount, price, weight, created);
        Address = deliveryAddress;
        CreatedRegion = createdRegion;
    }
}
