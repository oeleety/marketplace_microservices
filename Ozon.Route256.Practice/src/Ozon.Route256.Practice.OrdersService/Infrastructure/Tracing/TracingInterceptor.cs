using Grpc.Core.Interceptors;
using System.Diagnostics;
using Grpc.Core;
using Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Tracing;

public class TracingInterceptor : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        using var grpcActivity = Diagnostics.ActivitySource.StartActivity(
            name: context.Method,
            kind: ActivityKind.Internal,
            tags: new List<KeyValuePair<string, object?>>
            {
                new ("grpc_request", request),
                new ("grpc_headers", context.RequestHeaders)
            });

        return base.UnaryServerHandler(request, context, continuation);
    }
}
