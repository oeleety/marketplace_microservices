using Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Tracing;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Metrics;
using Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;
using Ozon.Route256.Practice.Shared;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddKafkaConsumer(configuration)
            .AddGrpcReflection()
            .AddGrpc(o =>
            {
                o.Interceptors.Add<LoggerInterceptor>();
                o.Interceptors.Add<TracingInterceptor>();
                o.Interceptors.Add<MetricsInterceptor>();
            });
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
            .AddAspNetCoreInstrumentation()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(nameof(OrdersService)))
            .AddNpgsql()
            .AddConsoleExporter()
            .AddSource(Diagnostics.ActivitySourceName)
            .AddJaegerExporter(options =>
            {
                options.AgentHost = configuration.TryGetValue("ROUTE256_JAEGER");
                options.AgentPort = 6831;
                options.Protocol = JaegerExportProtocol.UdpCompactThrift;
                options.ExportProcessorType = ExportProcessorType.Simple;
            }));
        services.AddSingleton<IGrpcMetrics, GrpcMetrics>();
        services.AddSingleton<IOrdersMetrics, OrdersMetrics>();
        return services;
    }
}