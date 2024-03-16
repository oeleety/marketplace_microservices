using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ozon.Route256.Practice.GatewayService;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>()
            .ConfigureKestrel(option =>
            {
                option.ListenPortByOptions(ProgramExtension.ROUTE256_HTTP_PORT, HttpProtocols.Http1);
            })
            )
    .Build()
    .RunAsync();

public static class ProgramExtension
{
    public const string ROUTE256_HTTP_PORT = "ROUTE256_HTTP_PORT";

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