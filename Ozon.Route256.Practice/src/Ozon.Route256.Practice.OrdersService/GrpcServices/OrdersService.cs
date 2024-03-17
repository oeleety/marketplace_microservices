using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.Exceptions;
using System.Linq;


namespace Ozon.Route256.Practice.OrdersService.GrpcServices;

public sealed class OrdersService : Orders.OrdersBase
{
    public override Task<CancelOrderResponse> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        throw new NotFoundException($"Order with id = {request.Id} not found");
    }
    public override Task<GetOrderStatusResponse> GetOrderStatus(GetOrderStatusRequest request, ServerCallContext context)
    {
        throw new NotFoundException($"Order with id = {request.Id} not found");
    }

    public override Task<GetRegionsResponse> GetRegions(GetRegionsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetRegionsResponse
        {
            Regions = { new List<Region>() }
        });
    }

    public override Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetOrdersResponse
        {
            Orders = { new List<Order>() }
        });
    }

    public override Task<GetOrdersByCustomerResponse> GetOrdersByCustomer(GetOrdersByCustomerRequest request, ServerCallContext context)
    {
        if (false)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Customer with id = {request.CustomerId} not found"));
        }
        return Task.FromResult(new GetOrdersByCustomerResponse
        {
            Orders = { new List<Order>() }
        });
    }

    public override Task<GetAggregatedOrdersByRegionResponse> GetAggregatedOrdersByRegion(GetAggregatedOrdersByRegionRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetAggregatedOrdersByRegionResponse
        {
            Regions = { new List<Region>()},
            StatByRegion = { new Dictionary<int, OrdersStat>()}
        });
    }
}
