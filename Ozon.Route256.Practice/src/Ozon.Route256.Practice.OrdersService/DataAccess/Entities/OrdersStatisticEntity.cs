namespace Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

public record OrdersStatisticEntity(
    string Region,
    int OrdersCount,
    decimal Price,
    double Weight,
    int CustomersCount);