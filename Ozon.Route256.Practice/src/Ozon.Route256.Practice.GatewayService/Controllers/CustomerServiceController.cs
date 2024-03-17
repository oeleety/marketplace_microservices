using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Ozon.Route256.Practice.GatewayService.Controllers;

[Route("api/customer-service/")]
[ApiController]
public class CustomerServiceController : ControllerBase
{
    private readonly ILogger<CustomerServiceController> _logger;
    private readonly Customers.CustomersClient _client;

    public CustomerServiceController(
        ILogger<CustomerServiceController> logger, 
        Customers.CustomersClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        try
        {
            var response = await _client.GetCustomersAsync(new GetCustomersRequest());
            _logger.LogInformation($"{nameof(GetCustomers)}. Got response from GRPC CustomerService. ");
            return Ok(response);
        }
        catch (RpcException ex)
        {
            _logger.LogError($"{nameof(GetCustomers)}. " +
                $"Got exception from GRPC CustomerService. RpcException :  {ex.Status.StatusCode}, {ex}");
            if (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound();
            }
            else
            {
                _logger.LogError($"{nameof(GetCustomers)}. Got exception from GRPC CustomerService. RpcException:  {ex}");
                throw new NotImplementedException();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(GetCustomers)}. Got exception from GRPC CustomerService. Exception:  {ex}");
            throw new NotImplementedException();
        }
    }
}
