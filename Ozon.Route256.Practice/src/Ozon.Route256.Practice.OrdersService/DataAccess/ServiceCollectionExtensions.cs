namespace Ozon.Route256.Practice.OrdersService.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection collection)
    {
        collection.AddScoped<IOrdersRepository, OrdersRepository>();
        collection.AddScoped<RedisCustomersCache>();
        collection.AddScoped<RedisOrdersRepository>();

        return collection;
    }
}