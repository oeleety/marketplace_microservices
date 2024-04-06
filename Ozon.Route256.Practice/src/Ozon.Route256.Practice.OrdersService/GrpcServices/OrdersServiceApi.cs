using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;
using Ozon.Route256.Practice.OrdersService.Proto;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

public sealed class OrdersServiceApi : Orders.OrdersBase
{
    private readonly Bll.IOrdersService _ordersService;

    public OrdersServiceApi(
        Bll.IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    public override async Task<CancelOrderResponse> CancelOrder(
        CancelOrderRequest request, 
        ServerCallContext context)
    {
        await _ordersService.CancelOrderAsync(request.Id, context.CancellationToken);
        return new CancelOrderResponse
        {
            Success = true
        };
    }

    public override async Task<GetOrderStatusResponse> GetOrderStatus(
        GetOrderStatusRequest request, 
        ServerCallContext context)
    {
        var status = await _ordersService.GetOrderStatusAsync(
            request.Id, context.CancellationToken);
        return new GetOrderStatusResponse { Status = From(status) };
    }

    public override async Task<GetRegionsResponse> GetRegions(
        GetRegionsRequest request, 
        ServerCallContext context)
    {
        var regions = await _ordersService.GetRegions(context.CancellationToken);
        var result = new GetRegionsResponse
        {
            Regions = { regions.Select(From).ToArray() }
        };

        return result;
    }

    public override async Task<GetOrdersResponse> GetOrders(
        GetOrdersRequest request, 
        ServerCallContext context)
    {
        var orders = await _ordersService.GetOrdersAsync(
            request.Regions.Select(From).ToArray(),
            From(request.OrderType),
            From(request.Pagination),
            context.CancellationToken,
            From(request.SortOrder),
            From(request.ValueOrder));
        return new GetOrdersResponse
        {
            Orders = { orders.Select(From).ToArray() }
        };
    }

    public override async Task<GetOrdersByCustomerResponse> GetOrdersByCustomer(
        GetOrdersByCustomerRequest request, 
        ServerCallContext context)
    {
        var orders = await _ordersService.GetOrdersByCustomer(
            request.CustomerId, 
            request.SinceTimestamp.ToDateTime(), 
            From(request.Pagination),
            context.CancellationToken);
        return new  GetOrdersByCustomerResponse
        {
            Orders = { orders.Select(From).ToArray() }
        };
    }

    public override async Task<GetAggregatedOrdersByRegionResponse> GetAggregatedOrdersByRegion(
        GetAggregatedOrdersByRegionRequest request, 
        ServerCallContext context)
    {
        var result = await _ordersService.GetAggregatedOrdersByRegion(
            request.Regions.Select(From).ToArray(),
            request.SinceTimestamp.ToDateTime(),
            context.CancellationToken);
        
        return new GetAggregatedOrdersByRegionResponse
        {
            StatisticByRegion = { result.Select(From).ToArray() }
        };
    }

    private static Order From(OrderEntity order) => new()
    {
        Id = order.Id,
        Status = From(order.OrderStatus),
        Type = From(order.OrderType),
        CustomerId = order.CustomerId,
        CustomerFullName = order.CustomerFullName,
        CustomerMobileNumber = order.CustomerMobileNumber,
        DeliveryAddress = From(order.DeliveryAddress),
        ItemsCount = order.ItemsCount,
        Price = (double)order.Price,
        Weight = order.Weight,
        Created = order.Created.ToTimestamp(),
        CreatedRegion = From(order.CreatedRegion),
    };

    private static Address From(AddressEntity address) => new()
    {
        Region = address.Region,
        City = address.City,
        Street = address.Street,
        Building = address.Building,
        Apartment = address.Apartment,
        Latitude = address.Latitude,
        Longitude = address.Longitude
    };

    private static Region From(RegionEntity region) => new()
    {
        Name = region.Name,
    };

    private static RegionEntity From(Region region) => new(region.Name);

    private static PaginationEntity From(Pagination p) => new(p.Offset, p.Limit);

    private static OrdersStatistic From(OrdersStatisticEntity ordersStatisticEntity) => new()
    {
        Region = ordersStatisticEntity.Region,
        OrdersCount = ordersStatisticEntity.OrdersCount,
        Weight = ordersStatisticEntity.Weight,
        Price = (double)ordersStatisticEntity.Price,
        CustomersCount = ordersStatisticEntity.CustomersCount,
    };

    private static OrderStatus From(OrderStatusEntity orderStatusEntity) =>
        orderStatusEntity switch
        {
            OrderStatusEntity.Created => OrderStatus.Created,
            OrderStatusEntity.SentToCustomer => OrderStatus.SentToCustomer,
            OrderStatusEntity.Lost => OrderStatus.Lost,
            OrderStatusEntity.Delivered => OrderStatus.Delivered,
            OrderStatusEntity.Cancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatusEntity), orderStatusEntity, null)
        };

    private static OrderStatusEntity From(OrderStatus orderStatus) =>
        orderStatus switch
        {
            OrderStatus.Created => OrderStatusEntity.Created,
            OrderStatus.SentToCustomer => OrderStatusEntity.SentToCustomer,
            OrderStatus.Lost => OrderStatusEntity.Lost,
            OrderStatus.Delivered => OrderStatusEntity.Delivered,
            OrderStatus.Cancelled => OrderStatusEntity.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };

    private static OrderTypeEntity From(OrderType orderType) => orderType switch
    {
        OrderType.Api => OrderTypeEntity.Api,
        OrderType.Mobile => OrderTypeEntity.Mobile,
        OrderType.Web => OrderTypeEntity.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };

    private static OrderType From(OrderTypeEntity orderTypeEntity) => orderTypeEntity switch
    {
        OrderTypeEntity.Api => OrderType.Api,
        OrderTypeEntity.Mobile => OrderType.Mobile,
        OrderTypeEntity.Web => OrderType.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderTypeEntity), orderTypeEntity, null)
    };

    private static ValueOrderEntity From(ValueOrder valueOrder) => valueOrder switch
    {
        ValueOrder.None => ValueOrderEntity.None,
        ValueOrder.Region => ValueOrderEntity.Region,
        ValueOrder.Status => ValueOrderEntity.Status,

        _ => throw new ArgumentOutOfRangeException(nameof(valueOrder), valueOrder, null)
    };

    private static SortOrderEntity From(SortOrder sortOrder) => sortOrder switch
    {
        SortOrder.Asc => SortOrderEntity.Asc,
        SortOrder.Desc => SortOrderEntity.Desc,

        _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
    };
}
