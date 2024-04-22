using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Application.Helpers;
using Ozon.Route256.Practice.OrdersService.Domain;
using Google.Protobuf.WellKnownTypes;
using Ozon.Route256.Practice.OrdersService.Proto;
using OrderProto = Ozon.Route256.Practice.OrdersService.Proto.Order;
using OrdersStatisticProto = Ozon.Route256.Practice.OrdersService.Proto.OrdersStatistic;
using AddressProto = Ozon.Route256.Practice.OrdersService.Proto.Address;
using RegionProto = Ozon.Route256.Practice.OrdersService.Proto.Region;
using PaginationProto = Ozon.Route256.Practice.OrdersService.Proto.Pagination;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

internal static class Mappers
{
    internal static OrderProto From(OrderAggregate order) => new()
    {
        Id = order.Order.Id,
        Status = From(order.Order.OrderStatus),
        Type = From(order.Order.OrderType),
        CustomerId = order.Order.CustomerId,
        CustomerFullName = order.Order.CustomerFullName,
        CustomerMobileNumber = order.Order.CustomerMobileNumber,
        DeliveryAddress = From(order.Address),
        ItemsCount = order.Order.ItemsCount,
        Price = (double)order.Order.Price,
        Weight = order.Order.Weight,
        Created = order.Order.CreatedAt.ToTimestamp(),
        CreatedRegion = From(order.CreatedRegion),
    };

    internal static AddressProto From(Domain.Address address) => new()
    {
        Region = address.Region,
        City = address.City,
        Street = address.Street,
        Building = address.Building,
        Apartment = address.Apartment,
        Latitude = address.Coordinates.Latitude,
        Longitude = address.Coordinates.Longitude
    };

    internal static RegionProto From(Domain.Region region) => new()
    {
        Name = region.Name,
    };

    internal static Domain.Region From(RegionProto region) => new(region.Name);

    internal static Application.Bll.Pagination From(PaginationProto p) => new(p.Offset, p.Limit);

    internal static OrdersStatisticProto From(OrdersStatisticEntity ordersStatisticEntity) => new()
    {
        Region = ordersStatisticEntity.Region,
        OrdersCount = ordersStatisticEntity.OrdersCount,
        Weight = ordersStatisticEntity.Weight,
        Price = (double)ordersStatisticEntity.Price,
        CustomersCount = ordersStatisticEntity.CustomersCount,
    };

    internal static OrderStatus From(OrderStatusModel orderStatusEntity) =>
        orderStatusEntity switch
        {
            OrderStatusModel.Created => OrderStatus.Created,
            OrderStatusModel.SentToCustomer => OrderStatus.SentToCustomer,
            OrderStatusModel.Lost => OrderStatus.Lost,
            OrderStatusModel.Delivered => OrderStatus.Delivered,
            OrderStatusModel.Cancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatusEntity), orderStatusEntity, null)
        };

    internal static OrderTypeModel From(OrderType orderType) => orderType switch
    {
        OrderType.Api => OrderTypeModel.Api,
        OrderType.Mobile => OrderTypeModel.Mobile,
        OrderType.Web => OrderTypeModel.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };

    internal static OrderType From(OrderTypeModel orderTypeEntity) => orderTypeEntity switch
    {
        OrderTypeModel.Api => OrderType.Api,
        OrderTypeModel.Mobile => OrderType.Mobile,
        OrderTypeModel.Web => OrderType.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderTypeEntity), orderTypeEntity, null)
    };

    internal static ValueOrderDto From(ValueOrder valueOrder) => valueOrder switch
    {
        ValueOrder.None => ValueOrderDto.None,
        ValueOrder.Region => ValueOrderDto.Region,
        ValueOrder.Status => ValueOrderDto.Status,

        _ => throw new ArgumentOutOfRangeException(nameof(valueOrder), valueOrder, null)
    };

    internal static SortOrderDto From(SortOrder sortOrder) => sortOrder switch
    {
        SortOrder.Asc => SortOrderDto.Asc,
        SortOrder.Desc => SortOrderDto.Desc,

        _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
    };
}
