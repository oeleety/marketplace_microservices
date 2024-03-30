using Ozon.Route256.Practice.OrdersService.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.Infrastructure;
using Ozon.Route256.Practice.OrdersService.GrpcClients;
using Ozon.Route256.Practice.Shared;
using Ozon.Route256.Practice.OrdersService.Configuration;
using Ozon.Route256.Practice.OrdersService.DataAccess;
using Ozon.Route256.Practice.OrdersService.CachedClients;

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
        serviceCollection.AddControllers();
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddGrpcClient<SdService.SdServiceClient>(option =>
        {
            option.Address = new Uri(_configuration.TryGetValue("ROUTE256_SD_ADDRESS"));
        });
        serviceCollection.AddSingleton<IDbStore, DbStore>();
        serviceCollection.AddHostedService<SdConsumerHostedService>();
        serviceCollection
            .AddSettings(_configuration)
            .AddGrpcClients(_configuration)
            .AddInfrastructure(_configuration)
            .AddRepositories()
            .AddCachedClients();

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
