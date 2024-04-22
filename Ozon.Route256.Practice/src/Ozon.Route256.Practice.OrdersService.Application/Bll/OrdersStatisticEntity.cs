namespace Ozon.Route256.Practice.OrdersService.Application.Helpers;

public record OrdersStatisticEntity(
    string Region,
    int OrdersCount,
    decimal Price,
    double Weight,
    int CustomersCount);