using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Repositories;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Practice.Shared;
using Microsoft.Extensions.Configuration;
using Ozon.Route256.Practice.OrdersService.Application.Bll;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPg(this IServiceCollection services,
        IConfiguration configuration, 
        System.Reflection.Assembly assembly)
    {
        var connectionString = configuration.TryGetValue("ROUTE256_OS_DB");
        services.AddFluentMigratorCore()
            .ConfigureRunner(
                builder => builder
                    .AddPostgres()
                    .ScanIn(assembly)
                    .For.Migrations())
            .AddOptions<ProcessorOptions>()
            .Configure(
                options =>
                {
                    options.ProviderSwitches = "Force Quote=false";
                    options.Timeout = TimeSpan.FromMinutes(10);
                    options.ConnectionString = connectionString;
                });
        services.AddScoped<IOrdersRepository, ShardOrdersRepositoryPg>();
        services.AddScoped<IRegionsRepository, ShardRegionsRepositoryPg>();
        services.AddScoped<IAddressesRepository, ShardAddressesRepositoryPg>();
        services.AddScoped<IOrdersAggregateRepository, OrdersAggregateRepository>();
        services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));
        services.AddSingleton<IShardPostgresConnectionFactory, ShardConnectionFactory>();
        services.AddSingleton<IShardingRule<long>, LongShardingRule>();
        services.AddSingleton<IShardMigrator, ShardMigrator>();

        return services;
    }
}