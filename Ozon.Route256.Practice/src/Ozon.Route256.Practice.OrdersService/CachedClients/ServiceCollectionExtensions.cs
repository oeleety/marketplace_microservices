namespace Ozon.Route256.Practice.OrdersService.CachedClients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachedClients(this IServiceCollection collection)
    {
        collection.AddScoped<CachedCustomersClient>();

        return collection;
    }
}