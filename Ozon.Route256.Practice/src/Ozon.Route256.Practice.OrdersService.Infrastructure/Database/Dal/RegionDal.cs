using NpgsqlTypes;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

public record RegionDal(
    string Name,
    NpgsqlPoint DepotLatLon);
