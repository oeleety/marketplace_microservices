using Ozon.Route256.Practice.OrdersService.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.Infrastructure;
using Ozon.Route256.Practice.LogisticsSimulator.Grpc;
using Ozon.Route256.Practice.OrdersService.GrpcClients;
using Ozon.Route256.Practice.Shared;
using Ozon.Route256.Practice.Proto;

namespace Ozon.Route256.Practice.OrdersService;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddGrpc(option => option.Interceptors.Add<LoggerInterceptor>());
        serviceCollection.AddGrpcClient<SdService.SdServiceClient>(option =>
        {
            option.Address = new Uri(_configuration.TryGetValue("ROUTE256_SD_ADDRESS"));
        });
        serviceCollection.AddGrpcClient<LogisticsSimulatorService.LogisticsSimulatorServiceClient>(option =>
        {
            option.Address = new Uri(_configuration.TryGetValue("ROUTE256_LS_ADDRESS"));
        });
        serviceCollection.AddGrpcClient<Customers.CustomersClient>(option =>
        {
            option.Address = new Uri(_configuration.TryGetValue("ROUTE256_CUSTOMER_ADDRESS"));
        });
        serviceCollection.AddSingleton<LogisticsSimulatorClient>();
        serviceCollection.AddSingleton<CustomersServiceClient>();
        serviceCollection.AddControllers();
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddGrpcReflection();
        serviceCollection.AddScoped<IOrdersRepository, OrdersRepository>();
        serviceCollection.AddSingleton<IDbStore, DbStore>();
        serviceCollection.AddHostedService<SdConsumerHostedService>();
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();
        applicationBuilder.UseEndpoints(endpointRouteBuilder =>
        {
            endpointRouteBuilder.MapGrpcService<GrpcServices.OrdersService>();
            endpointRouteBuilder.MapGrpcReflectionService();
        });
    }
}
