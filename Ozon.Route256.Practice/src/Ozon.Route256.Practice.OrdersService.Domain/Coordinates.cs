using Ozon.Route256.Practice.OrdersService.Domain.Core;

namespace Ozon.Route256.Practice.OrdersService.Domain;

public sealed class Coordinates : ValueObject
{
    public Coordinates(double latitude, double longitude)
    {
        if (latitude > 90 || latitude < -90)
        {
            throw new DomainException("Incorrect latitude");
        }
        if (longitude > 180 || longitude < -180)
        {
            throw new DomainException("Incorrect longitude");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
