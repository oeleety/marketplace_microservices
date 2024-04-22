namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public interface ICachedCustomersClient
{
    Task EnsureExistsAsync(int id, CancellationToken cancellationToken);
}