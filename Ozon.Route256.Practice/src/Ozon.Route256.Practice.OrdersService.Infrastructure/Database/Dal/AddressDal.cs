using NpgsqlTypes;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;

public record AddressDal(
    int Id,
    string RegionName,
    string City,
    string Street,
    string Building,
    string Apartment,
    NpgsqlPoint CoordinateLatLon,
    long OrderId);

public record AddressDalToInsert(
    string RegionName,
    string City,
    string Street,
    string Building,
    string Apartment,
    NpgsqlPoint CoordinateLatLon,
    long OrderId);