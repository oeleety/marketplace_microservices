using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using Ozon.Route256.Practice.OrdersService;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(option =>
            {
                option.ListenPortByOptions(ProgramExtension.Route256GrpcPort, HttpProtocols.Http2);
            }))
    .Build()
    .RunAsync();

public static class ProgramExtension// todo extract to project in order to duplicate with the same code in orders project
{
    public const string Route256GrpcPort = "ROUTE256_GRPC_PORT";

    public static void ListenPortByOptions(
        this KestrelServerOptions option,
        string envOption,
        HttpProtocols httpProtocol)
    {
        var isHttpPortParsed = int.TryParse(Environment.GetEnvironmentVariable(envOption), out var httpPort);

        if (isHttpPortParsed)
        {
            option.Listen(IPAddress.Any, httpPort, options => options.Protocols = httpProtocol);
        }
    }
}
