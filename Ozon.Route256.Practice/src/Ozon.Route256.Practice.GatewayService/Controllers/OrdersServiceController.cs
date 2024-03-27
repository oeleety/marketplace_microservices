using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Practice.OrdersService.Proto;

namespace Ozon.Route256.Practice.GatewayService.Controllers;

[Route("api/orders-service/")]
[ApiController]
public class OrdersServiceController : ControllerBase
{
    private readonly ILogger<OrdersServiceController> _logger;
    private readonly Orders.OrdersClient _client;

    public OrdersServiceController(
        ILogger<OrdersServiceController> logger, 
        Orders.OrdersClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpPost("order/cancel")]
    public async Task<IActionResult> CancelOrder(
        [FromBody] CancelOrderRequest request)
    {
        try
        {
            var response = await _client.CancelOrderAsync(request);
            _logger.LogInformation($"{nameof(CancelOrder)}. Got response from GRPC OrdersService : success = {response.Success}");
            return Ok(response);
        }
        catch(RpcException ex)
        {
            _logger.LogError(ex, $"{nameof(CancelOrder)}. Got exception from GRPC OrdersService");
            return ex.Status.StatusCode switch
            {
                Grpc.Core.StatusCode.FailedPrecondition => BadRequest(new CancelOrderResponse { Success = false, Error = ex.Status.Detail} ),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Status.Detail })
            };
        }
    }

    [HttpGet("order/status/{id}")]
    public async Task<IActionResult> GetOrderStatus(int id)
    {
        var response = await _client.GetOrderStatusAsync(new GetOrderStatusRequest { Id = id});
        _logger.LogInformation($"{nameof(GetOrderStatus)}. Got response from GRPC OrdersService : status = {response.Status}");
        return Ok(response);
    }

    [HttpPost("orders/list")]
    public async Task<IActionResult> GetOrders(
        [FromBody] GetOrdersRequest request)
    {
        var response = await _client.GetOrdersAsync(request);
        _logger.LogInformation($"{nameof(GetOrders)}. Got response from GRPC OrdersService.");
        return Ok(response);
    }

    [HttpPost("orders/list/by-customer")]
    public async Task<IActionResult> GetOrdersByCustomer(
        [FromBody] GetOrdersByCustomerRequest request)
    {
        var response = await _client.GetOrdersByCustomerAsync(request);
        _logger.LogInformation($"{nameof(GetOrdersByCustomer)}. Got response from GRPC OrdersService.");
        return Ok(response);
    }

    [HttpPost("regions/list")]
    public async Task<IActionResult> GetRegions(
        [FromBody] GetRegionsRequest request)
    {
        var response = await _client.GetRegionsAsync(request);
        _logger.LogInformation($"{nameof(GetRegions)}. Got response from GRPC OrdersService.");
        return Ok(response);
    }

    [HttpPost("orders/list/aggregate/by-region")]
    public async Task<IActionResult> GetAggregatedOrdersByRegion(
        [FromBody] GetAggregatedOrdersByRegionRequest request)
    {
        var response = await _client.GetAggregatedOrdersByRegionAsync(request);
        _logger.LogInformation($"{nameof(GetAggregatedOrdersByRegion)}. Got response from GRPC OrdersService");
        return Ok(response);
    }
}
