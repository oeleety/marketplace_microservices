using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Ozon.Route256.Practice.OrdersService;

namespace Ozon.Route256.Practice.GatewayService;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var factory = new StaticResolverFactory(address => new[]
{
             new BalancerAddress("orders-service-1", 5005),
             new BalancerAddress("orders-service-2", 5005)
        });

        serviceCollection.AddControllers();
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddSingleton<ResolverFactory>(factory);

        serviceCollection.AddGrpcClient<Orders.OrdersClient>(options =>
        {
            options.Address = new Uri("static:///orders-service");
        }).ConfigureChannel(x =>
        {
            x.Credentials = ChannelCredentials.Insecure;
            x.ServiceConfig = new ServiceConfig
            {
                LoadBalancingConfigs = { new LoadBalancingConfig("round_robin") }
            };
        });

        serviceCollection.AddGrpcClient<Customers.CustomersClient>(option =>
        {
            var url = _configuration.GetValue<string>("ROUTE256_CUSTOMER_ADDRESS");
            if (string.IsNullOrWhiteSpace(url))
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
