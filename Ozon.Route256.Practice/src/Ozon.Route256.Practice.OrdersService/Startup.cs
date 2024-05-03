using Ozon.Route256.Practice.OrdersService.Application;
using Ozon.Route256.Practice.OrdersService.Infrastructure;
using Prometheus;
using Serilog;

namespace Ozon.Route256.Practice.OrdersService;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithMemoryUsage()
            .CreateLogger();
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllers();
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddInfrastructure(_configuration);
        serviceCollection.AddInfrastructureLayer(_configuration, GetType().Assembly);
        serviceCollection.AddApplicationLayer();
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();
        applicationBuilder.UseEndpoints(endpointRouteBuilder =>
        {
            endpointRouteBuilder.MapMetrics();
            endpointRouteBuilder.MapGrpcService<GrpcServices.OrdersServiceApi>();
            endpointRouteBuilder.MapGrpcReflectionService();
        });
    }
}
