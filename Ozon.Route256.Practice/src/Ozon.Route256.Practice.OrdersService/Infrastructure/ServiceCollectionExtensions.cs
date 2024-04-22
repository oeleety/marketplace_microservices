using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddKafkaConsumer(configuration)
            .AddGrpcReflection()
            .AddGrpc(o => o.Interceptors.Add<LoggerInterceptor>());

        return services;
    }
}