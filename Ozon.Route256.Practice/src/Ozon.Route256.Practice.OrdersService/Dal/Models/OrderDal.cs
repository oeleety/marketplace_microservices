namespace Ozon.Route256.Practice.OrdersService.Dal.Models;

public record OrderDal(
    long Id,
    OrderStatus OrderStatus,
    OrderType OrderType,
    int CustomerId,
    string CustomerFullName,
    string CustomerMobileNumber,
    int AddressId,
    int ItemsCount,
    decimal Price,
    float Weight,
    DateTime Created,
    string RegionName);

public enum OrderStatus
{
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled,
    PreOrder,
}

public enum OrderType
{
    Web,
    Api,
    Mobile
}

public enum ValueOrderDal
{
    None,
    Region,
    Status,
}