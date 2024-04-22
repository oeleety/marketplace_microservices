namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

public record OrderDal(
    long Id,
    OrderStatusDal OrderStatus,
    OrderTypeDal OrderType,
    int CustomerId,
    string CustomerFullName,
    string CustomerMobileNumber,
    int AddressId, 
    int ItemsCount,
    decimal Price,
    double Weight,
    DateTime Created,
    string RegionName);

public enum OrderStatusDal
{
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled,
    PreOrder,
}

public enum OrderTypeDal
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