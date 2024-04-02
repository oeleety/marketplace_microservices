using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Practice.Proto;

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
        var response = await _client.GetCustomersAsync(new GetCustomersRequest());
        _logger.LogInformation($"{nameof(GetCustomers)}. Got response from GRPC CustomerService. ");
        return Ok(response);
    }
}
