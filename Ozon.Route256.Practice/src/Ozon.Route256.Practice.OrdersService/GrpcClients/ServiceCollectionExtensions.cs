using Ozon.Route256.Practice.LogisticsSimulator.Grpc;
using Ozon.Route256.Practice.Proto;
using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.OrdersService.GrpcClients;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcClients(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddGrpcClient<LogisticsSimulatorService.LogisticsSimulatorServiceClient>(configuration, "ROUTE256_LS_ADDRESS");
        serviceCollection.AddGrpcClient<Customers.CustomersClient>(configuration, "ROUTE256_CUSTOMER_ADDRESS");
        serviceCollection.AddSingleton<LogisticsSimulatorClient>();
        serviceCollection.AddSingleton<CustomersServiceClient>();

        return serviceCollection;
    }

    private static IServiceCollection AddGrpcClient<TClient>(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        string key) where TClient : class
    {
        serviceCollection.AddGrpcClient<TClient>(option =>
        {
            option.Address = new Uri(configuration.TryGetValue(key));
        });

        return serviceCollection;
    }
}