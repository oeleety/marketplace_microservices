using Ozon.Route256.Practice.OrdersService;

namespace Ozon.Route256.Practice.GatewayService
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddControllers();
            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddSwaggerGen();
            serviceCollection.AddGrpcClient<Orders.OrdersClient>(option =>
            {
                var url = _configuration.GetValue<string>("ROUTE256_ORDERS_ADDRESS");
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException("ROUTE256_ORDERS_ADDRESS variable is null or empty");
                }

                option.Address = new Uri(url);
            });
            serviceCollection.AddGrpcClient<Customers.CustomersClient>(option =>
            {
                var url = _configuration.GetValue<string>("ROUTE256_CUSTOMER_ADDRESS");
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException("ROUTE256_CUSTOMER_ADDRESS variable is null or empty");
                }

                option.Address = new Uri(url);
            });
        }

        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseRouting();
            applicationBuilder.UseSwagger();
            applicationBuilder.UseSwaggerUI();
            applicationBuilder.UseEndpoints(x => x.MapControllers());
        }
    }
}
