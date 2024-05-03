using Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;
using Ozon.Route256.Practice.OrdersService.Domain;
using Prometheus;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Metrics;

internal class OrdersMetrics : IOrdersMetrics
{
    private readonly Counter _cancelledCounter = Prometheus.Metrics.CreateCounter(
        name: "orders_service_orders_cancelled_from_status",
        help: "Отмена заказа с статусов (произошла)",
        "status");

    private readonly Counter _cancellationTriesCounter = Prometheus.Metrics.CreateCounter(
        name: "orders_service_orders_cancellation_result",
        help: "Запросы на отмена заказа с статусов",
        "status");

    public void CancellationTried(bool success) 
        => _cancellationTriesCounter.WithLabels(success.ToString()).Inc();

    public void OrderCancelled(OrderStatusModel status)
        =>_cancelledCounter.WithLabels(status.ToString()).Inc();    
}
