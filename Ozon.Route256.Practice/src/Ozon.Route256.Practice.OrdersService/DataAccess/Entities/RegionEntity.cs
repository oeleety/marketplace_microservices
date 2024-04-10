namespace Ozon.Route256.Practice.OrdersService.DataAccess.Entities;

public sealed class RegionEntity
{
    public RegionEntity(string name)
    {
        Name = name;
    }

    public RegionEntity(string name, (double lat, double lon) depot)
    {
        Name = name;
        Depot = depot;
    }

    public string Name { get; private set; }
    public (double lat, double lon) Depot { get; private set; }
}