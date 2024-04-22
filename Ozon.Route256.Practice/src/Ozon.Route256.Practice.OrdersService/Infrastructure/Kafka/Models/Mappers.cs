using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Kafka.Models;

internal static class Mappers
{
    internal static OrderAggregate From(PreOrder preOrder, Proto.Customer customer) =>
       new(
           preOrder.Id,
           OrderStatusModel.PreOrder,
           From(preOrder.Source),
           customer.Id,
           customer.FirstName + " " + customer.LastName,
           customer.MobileNumber,
           From(preOrder.Customer.Address),
           preOrder.Goods.Sum(g => g.Quantity),
           preOrder.Goods.Sum(g => g.Price),
           preOrder.Goods.Sum(g => g.Weight),
           DateTime.UtcNow,
           new Region(preOrder.Customer.Address.Region)
       );

    internal static OrderTypeModel From(OrderSource source) => source switch
    {
        OrderSource.WebSite => OrderTypeModel.Web,
        OrderSource.Mobile => OrderTypeModel.Mobile,
        OrderSource.Api => OrderTypeModel.Api,
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };

    internal static Domain.Address From(Address a) => new(
        a.Region,
        a.City,
        a.Street,
        a.Building,
        a.Apartment,
        a.Latitude,
        a.Longitude);
}
