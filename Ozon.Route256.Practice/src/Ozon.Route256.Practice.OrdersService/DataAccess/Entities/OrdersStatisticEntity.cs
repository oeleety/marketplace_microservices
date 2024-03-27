namespace Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

public record OrdersStatisticEntity(
    string Region,
    int OrdersCount,
    double Price,
    double Weight,
    int CustomersCount);