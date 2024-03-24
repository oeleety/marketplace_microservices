using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ozon.Route256.Practice.GatewayService;
using Ozon.Route256.Practice.Shared;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder
        .UseStartup<Startup>()
        .ConfigureKestrel(option =>
        {
            option.ListenPortByOptions("ROUTE256_HTTP_PORT", HttpProtocols.Http1);
        }))
    .Build()
    .RunAsync();