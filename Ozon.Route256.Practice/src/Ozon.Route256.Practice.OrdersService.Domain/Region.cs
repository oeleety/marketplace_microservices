namespace Ozon.Route256.Practice.OrdersService.Domain;

public sealed class Region
{
    public Region(string name)
    {
        Name = name;
    }

    public Region(string name, Coordinates depot)
    {
        Name = name;
        Depot = depot;
    }

    public string Name { get; }
    public Coordinates Depot { get; }
}