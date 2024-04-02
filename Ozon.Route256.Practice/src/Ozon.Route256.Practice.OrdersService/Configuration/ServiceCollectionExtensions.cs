using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.OrdersService.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(o =>
        {
            o.Servers = configuration.TryGetValue("ROUTE256_KAFKA_BROKERS");
        });
        return services;
    }
}