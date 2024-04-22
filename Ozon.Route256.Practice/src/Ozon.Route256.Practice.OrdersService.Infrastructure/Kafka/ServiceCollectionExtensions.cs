using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Practice.Shared;
using StackExchange.Redis;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaProducer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaProducerSettings>(o =>
        {
            o.Servers = configuration.TryGetValue("ROUTE256_KAFKA_BROKERS");
        });
        services.AddSingleton<IKafkaProducer<long, string>, KafkaProducer>();
        services.AddSingleton<INewOrderProducer, NewOrderProducerService>();

        return services;
    }
}