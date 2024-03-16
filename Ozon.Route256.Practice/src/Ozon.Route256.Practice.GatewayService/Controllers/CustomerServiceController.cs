using Microsoft.AspNetCore.Mvc;

namespace Ozon.Route256.Practice.GatewayService.Controllers
{
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
        public IEnumerable<string> GetCustomers()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
