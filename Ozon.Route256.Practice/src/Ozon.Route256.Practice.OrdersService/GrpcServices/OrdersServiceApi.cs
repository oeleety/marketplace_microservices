using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Proto;

namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

internal sealed class OrdersServiceApi : Orders.OrdersBase
{
    private readonly IOrdersService _ordersService;

    public OrdersServiceApi(
        IOrdersService ordersService)
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
        return new GetOrderStatusResponse { Status = Mappers.From(status) };
    }

    public override async Task<GetRegionsResponse> GetRegions(
        GetRegionsRequest request,
        ServerCallContext context)
    {
        var regions = await _ordersService.GetRegionsAsync(context.CancellationToken);
        var result = new GetRegionsResponse
        {
            Regions = { regions.Select(Mappers.From).ToArray() }
        };

        return result;
    }

    public override async Task<GetOrdersResponse> GetOrders(
        GetOrdersRequest request, 
        ServerCallContext context)
    {
        var orders = await _ordersService.GetOrdersAsync(
            request.Regions.Select(Mappers.From).ToArray(),
            Mappers.From(request.OrderType),
            Mappers.From(request.Pagination),
            context.CancellationToken,
            Mappers.From(request.SortOrder),
            Mappers.From(request.ValueOrder));
        
        return new GetOrdersResponse
        {
            Orders = { orders.Select(Mappers.From).ToArray() }
        };
    }

    public override async Task<GetOrdersByCustomerResponse> GetOrdersByCustomer(
        GetOrdersByCustomerRequest request,
        ServerCallContext context)
    {
        var orders = await _ordersService.GetOrdersByCustomerAsync(
            request.CustomerId,
            request.SinceTimestamp.ToDateTime(),
            Mappers.From(request.Pagination),
            context.CancellationToken);
        
        return new GetOrdersByCustomerResponse
        {
            Orders = { orders.Select(Mappers.From).ToArray() }
        };
    }

    public override async Task<GetAggregatedOrdersByRegionResponse> GetAggregatedOrdersByRegion(
        GetAggregatedOrdersByRegionRequest request,
        ServerCallContext context)
    {
        var result = await _ordersService.GetAggregatedOrdersByRegionAsync(
            request.Regions.Select(Mappers.From).ToArray(),
            request.SinceTimestamp.ToDateTime(),
            context.CancellationToken);

        return new GetAggregatedOrdersByRegionResponse
        {
            StatisticByRegion = { result.Select(Mappers.From).ToArray() }
        };
    }
}
