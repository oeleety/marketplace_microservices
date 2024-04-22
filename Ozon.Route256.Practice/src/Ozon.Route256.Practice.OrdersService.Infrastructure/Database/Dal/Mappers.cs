using NpgsqlTypes;
using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

internal static class Mappers
{
    internal static Region From(RegionDal region) => new(
        region.Name,
        new(region.DepotLatLon.X, region.DepotLatLon.Y));

    internal static OrderAggregate From((OrderDal order, AddressDal address, RegionDal region) from)
        => new(From(from.order), From(from.address), From(from.region));

    internal static Order From(OrderDal order) => new(
        id: order.Id,
        orderStatus: From(order.OrderStatus),
        orderType: From(order.OrderType),
        customerId: order.CustomerId,
        customerFullName: order.CustomerFullName,
        customerMobileNumber: order.CustomerMobileNumber,
        itemsCount: order.ItemsCount,
        price: order.Price,
        weight: order.Weight,
        createdAt: order.Created);

    internal static OrderDal From(OrderAggregate order, int addressId) => new(
        Id: order.Order.Id,
        OrderStatus: From(order.Order.OrderStatus),
        OrderType: From(order.Order.OrderType),
        CustomerId: order.Order.CustomerId,
        CustomerFullName: order.Order.CustomerFullName,
        CustomerMobileNumber: order.Order.CustomerMobileNumber,
        AddressId: addressId,
        ItemsCount: order.Order.ItemsCount,
        Price: order.Order.Price,
        Weight: order.Order.Weight,
        Created: order.Order.CreatedAt,
        RegionName: order.CreatedRegion.Name);

    internal static Address From(AddressDal address) => new(
        address.RegionName,
        address.City,
        address.Street,
        address.Building,
        address.Apartment,
        latitude: address.CoordinateLatLon.X,
        longitude: address.CoordinateLatLon.Y);

    internal static AddressDalToInsert From(long orderId, Address address) => new(
        RegionName: address.Region,
        City: address.City,
        Street: address.Street,
        Building: address.Building,
        Apartment: address.Apartment,
        CoordinateLatLon: new NpgsqlPoint(address.Coordinates.Latitude, address.Coordinates.Longitude),
        OrderId: orderId);

    internal static OrderStatusModel From(OrderStatusDal orderStatus) =>
        orderStatus switch
        {
            OrderStatusDal.Created => OrderStatusModel.Created,
            OrderStatusDal.SentToCustomer => OrderStatusModel.SentToCustomer,
            OrderStatusDal.Lost => OrderStatusModel.Lost,
            OrderStatusDal.Delivered => OrderStatusModel.Delivered,
            OrderStatusDal.Cancelled => OrderStatusModel.Cancelled,
            OrderStatusDal.PreOrder => OrderStatusModel.PreOrder,

            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };

    internal static OrderStatusDal From(OrderStatusModel orderStatus) =>
        orderStatus switch
        {
            OrderStatusModel.Created => OrderStatusDal.Created,
            OrderStatusModel.SentToCustomer => OrderStatusDal.SentToCustomer,
            OrderStatusModel.Lost => OrderStatusDal.Lost,
            OrderStatusModel.Delivered => OrderStatusDal.Delivered,
            OrderStatusModel.Cancelled => OrderStatusDal.Cancelled,
            OrderStatusModel.PreOrder => OrderStatusDal.PreOrder,

            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };

    internal static OrderTypeModel From(OrderTypeDal orderType) => orderType switch
    {
        OrderTypeDal.Api => OrderTypeModel.Api,
        OrderTypeDal.Mobile => OrderTypeModel.Mobile,
        OrderTypeDal.Web => OrderTypeModel.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };

    internal static OrderTypeDal From(OrderTypeModel orderType) => orderType switch
    {
        OrderTypeModel.Api => OrderTypeDal.Api,
        OrderTypeModel.Mobile => OrderTypeDal.Mobile,
        OrderTypeModel.Web => OrderTypeDal.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };
}
