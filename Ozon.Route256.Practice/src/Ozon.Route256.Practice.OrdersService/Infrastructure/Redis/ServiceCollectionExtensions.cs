using StackExchange.Redis;
using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(
        _ =>
            {
                var address = configuration.TryGetValue("ROUTE256_REDIS");
                var connection = ConnectionMultiplexer.Connect(address);

                 return connection;
            });

        return services;
    }
}