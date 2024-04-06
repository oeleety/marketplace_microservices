using NpgsqlTypes;

namespace Ozon.Route256.Practice.OrdersService.Dal.Models;

public record RegionDal(
    string Name,
    NpgsqlPoint DepotLatLon);
