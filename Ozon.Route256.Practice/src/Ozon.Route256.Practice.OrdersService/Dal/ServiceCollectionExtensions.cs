using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Repositories;
using Ozon.Route256.Practice.Shared;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Dal;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPg(this IServiceCollection services,
        IConfiguration configuration, System.Reflection.Assembly assembly)
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
        services.AddSingleton<IPostgresConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));
        services.AddScoped<IOrdersRepositoryPg, ShardOrdersRepositoryPg>();
        services.AddScoped<IRegionsRepositoryPg, ShardRegionsRepositoryPg>();
        services.AddScoped<IAddressesRepositoryPg, ShardAddressesRepositoryPg>();
        services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));
        services.AddSingleton<IShardPostgresConnectionFactory, ShardConnectionFactory>();
        services.AddSingleton<IShardingRule<long>, LongShardingRule>();
        services.AddSingleton<IShardMigrator, ShardMigrator>();

        return services;
    }
}