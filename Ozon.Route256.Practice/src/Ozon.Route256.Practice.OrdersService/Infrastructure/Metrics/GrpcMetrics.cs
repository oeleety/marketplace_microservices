﻿using Prometheus;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Metrics;

internal class GrpcMetrics : IGrpcMetrics
{
    private readonly Histogram _histogram = Prometheus.Metrics.CreateHistogram(
        name: "orders_service_grpc_response_time",
        help: "Гистограма распределения времени ответа gRPC метода",
        labelNames: new []
        {
            "method",
            "is_success"
        },
        configuration: new HistogramConfiguration
        {
            Buckets = new []{ 0.001, 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 3, 5, },
        });

    public void WriteResponseTime(string method, long elapsedMs, bool isSuccess)
        => _histogram.WithLabels(method, isSuccess.ToString()).Observe(elapsedMs / 1000.0);
}
