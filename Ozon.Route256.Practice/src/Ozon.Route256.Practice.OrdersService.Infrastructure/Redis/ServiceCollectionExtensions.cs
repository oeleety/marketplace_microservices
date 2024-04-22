using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Redis;
using Ozon.Route256.Practice.Shared;
using StackExchange.Redis;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
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
        services.AddScoped<RedisCustomersCache>();

        return services;
    }
}