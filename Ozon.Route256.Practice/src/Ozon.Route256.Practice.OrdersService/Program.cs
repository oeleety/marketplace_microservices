using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ozon.Route256.Practice.OrdersService;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;
using Ozon.Route256.Practice.Shared;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(option =>
            {
                option.ListenPortByOptions("ROUTE256_GRPC_PORT", HttpProtocols.Http2);
            }))
    .Build()
    .RunOrMigrateAsync();

public static class ProgramExtension
{
    public static async Task RunOrMigrateAsync(
        this IHost host)
    {
        var option = Environment.GetEnvironmentVariable("MIGRATE_AND_RUN");
        if (!string.IsNullOrWhiteSpace(option))
        {
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IShardMigrator>();
            await runner.MigrateAsync(cancellationToken.Token);

            await host.RunAsync();
        }
        else
        {
            await host.RunAsync();
        }
    }
}