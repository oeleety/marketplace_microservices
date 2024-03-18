using Ozon.Route256.Practice.OrdersService.ClientBalancing;
using Ozon.Route256.Practice.OrdersService.Infrastructure;

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
            var url = _configuration.GetValue<string>("ROUTE256_SD_ADDRESS");
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("ROUTE256_SD_ADDRESS variable is null or empty");
            }

            option.Address = new Uri(url);
        });
        serviceCollection.AddControllers();
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
        serviceCollection.AddGrpcReflection();
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
