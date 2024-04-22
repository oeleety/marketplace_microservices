using Microsoft.Extensions.DependencyInjection;

namespace Ozon.Route256.Practice.OrdersService.Application;

public static class Startup
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<Bll.IOrdersService, Bll.OrdersService>();

        return services;
    }
}
