using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ozon.Route256.Practice.OrdersService;
using Ozon.Route256.Practice.Shared;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(option =>
            {
                option.ListenPortByOptions("ROUTE256_GRPC_PORT", HttpProtocols.Http2);
            }))
    .Build()
    .RunAsync();