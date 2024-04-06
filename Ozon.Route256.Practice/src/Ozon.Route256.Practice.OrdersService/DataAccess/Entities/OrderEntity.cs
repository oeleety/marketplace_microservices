namespace Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

public record OrderEntity(
    long Id, 
    OrderStatusEntity OrderStatus, 
    OrderTypeEntity OrderType, 
    int CustomerId, 
    string CustomerFullName, 
    string CustomerMobileNumber,
    AddressEntity DeliveryAddress, 
    int ItemsCount, 
    decimal Price, 
    double Weight, 
    DateTime Created, 
    RegionEntity CreatedRegion);

public record OrderEntityBase(
    long Id,
    OrderStatusEntity OrderStatus,
    OrderTypeEntity OrderType,
    int CustomerId,
    string CustomerFullName,
    string CustomerMobileNumber,
    int AddressId,
    int ItemsCount,
    decimal Price,
    double Weight,
    DateTime Created,
    string RegionName);

public enum OrderStatusEntity
{
    Created = 0,
    SentToCustomer = 1,
    Delivered = 2,
    Lost = 3,
    Cancelled = 4,
    PreOrder = 100,
}

public enum OrderTypeEntity
{
    Web = 0,
    Api = 1,
    Mobile = 2
}