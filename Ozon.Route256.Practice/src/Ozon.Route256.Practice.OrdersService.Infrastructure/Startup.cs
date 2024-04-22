using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;
using Ozon.Route256.Practice.Shared;
using Ozon.Route256.Practice.OrdersService.Infrastructure.GrpcClients;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services, 
        IConfiguration configuration,
        System.Reflection.Assembly assembly)
    {
        services.AddGrpcClient<SdService.SdServiceClient>(option =>
        {
            option.Address = new Uri(configuration.TryGetValue("ROUTE256_SD_ADDRESS"));
        });
        services.AddHostedService<SdConsumerHostedService>();
        services.AddScoped<ICachedCustomersClient, CachedCustomersClient>();
        services.AddScoped<CachedCustomersClient>();

        services.AddSingleton<IDbStore, DbStore>();
        services
            .AddGrpcClients(configuration)
            .AddPg(configuration, assembly)
            .AddKafkaProducer(configuration)
            .AddRedis(configuration)
            ;

        return services;
    }
}
