using Ozon.Route256.Practice.OrdersService.Domain.Core;

namespace Ozon.Route256.Practice.OrdersService.Domain;

public sealed class Order : Entity
{
    private static readonly HashSet<OrderStatusModel> ForbiddenToCancelStatus = new()
    {
        OrderStatusModel.Cancelled,
        OrderStatusModel.Delivered
    };

    public Order(
        long id,
        OrderStatusModel orderStatus,
        OrderTypeModel orderType, 
        int customerId, 
        string customerFullName, 
        string customerMobileNumber, 
        int itemsCount, 
        decimal price, 
        double weight, 
        DateTime createdAt) : base(id)
    {
        OrderStatus = orderStatus;
        OrderType = orderType;
        CustomerId = customerId;
        CustomerFullName = customerFullName;
        CustomerMobileNumber = customerMobileNumber;
        ItemsCount = itemsCount;
        Price = price;
        Weight = weight;
        CreatedAt = createdAt;
    }

    public OrderStatusModel OrderStatus { get; }
    public OrderTypeModel OrderType { get; }
    public int CustomerId { get; }
    public string CustomerFullName { get; }
    public string CustomerMobileNumber { get; }
    public int ItemsCount { get; }
    public decimal Price { get; }
    public double Weight { get; }
    public DateTime CreatedAt { get; }

    public bool CanCancel() => !ForbiddenToCancelStatus.Contains(OrderStatus);
}
