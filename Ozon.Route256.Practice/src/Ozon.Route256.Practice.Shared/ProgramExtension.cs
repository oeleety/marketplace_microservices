using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace Ozon.Route256.Practice.Shared;

public static class ProgramExtension
{
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

    public static string TryGetValue(
        this IConfiguration configuration,
        string key)
    {
        var value = configuration.GetValue<string>(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{key} variable is null or empty");
        }

        return value;
    }
}
