using System.Collections.Concurrent;
using Microsoft.OpenApi.Extensions;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;
using Ozon.Route256.Practice.OrdersService.Exceptions;

namespace Ozon.Route256.Practice.OrdersService.DataAccess;

internal class OrdersRepository : IOrdersRepository
{
    private static readonly ConcurrentDictionary<int, RegionEntity> Regions = new();
    private static readonly ConcurrentDictionary<long, OrderEntity> Orders = new();
    private static readonly HashSet<OrderStatusEntity> ForbiddenToCancelStatus = new()
    {
        OrderStatusEntity.Cancelled,
        OrderStatusEntity.Delivered
    };

    public OrdersRepository()
    {
        var moscow = new RegionEntity(1, "Moscow");
        var novosibirsk = new RegionEntity(2, "Novosibirsk");
        var spb = new RegionEntity(3, "StPetersburg");
        Regions.TryAdd(moscow.Id, moscow);
        Regions.TryAdd(novosibirsk.Id, novosibirsk);
        Regions.TryAdd(spb.Id, spb);

        var date = DateTime.ParseExact("2023-11-01 14:40:52,531", "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime(); // "seconds": "1698838852",
        var order1 = new OrderEntity(1, OrderStatusEntity.Created, OrderTypeEntity.Api, CustomerId: 1, 
            "Oks Mush", "89771451326", new AddressEntity("Moscow", "Zelenograd", "empty", "901", "81", 23d, 23d), 1, 1000, 1, 
            date, moscow);
        var order2 = order1 with { Id = 2, OrderStatus = OrderStatusEntity.SentToCustomer, OrderType = OrderTypeEntity.Web, CustomerId = 2 };
        var order3 = order1 with { Id = 3, OrderStatus = OrderStatusEntity.Delivered, OrderType = OrderTypeEntity.Mobile, Created = date.AddDays(5), CustomerId = 2};
        var order4 = order1 with { Id = 4, OrderStatus = OrderStatusEntity.Lost, Created = date.AddDays(-5), CreatedRegion = novosibirsk };
        var order5 = order1 with { Id = 5, OrderStatus = OrderStatusEntity.Cancelled, Created = DateTime.UtcNow.AddDays(-2), CreatedRegion = spb };
        Orders.TryAdd(order1.Id, order1);
        Orders.TryAdd(order2.Id, order2);
        Orders.TryAdd(order3.Id, order3);
        Orders.TryAdd(order4.Id, order4);
        Orders.TryAdd(order5.Id, order5);
    }

    public async Task CancelOrderAsync(long id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrder(id, token);
        if (ForbiddenToCancelStatus.Contains(order.OrderStatus))
        {
            throw new UnprocessableException($"Cannot cancel order with id={id} in state {order.OrderStatus}.");
        }
        else
        {
            var updatedOrder = order with { OrderStatus = OrderStatusEntity.Cancelled };

            token.ThrowIfCancellationRequested();

            Orders[order.Id] = updatedOrder;
        }
    }

    public async Task<OrderStatusEntity> GetOrderStatusAsync(long id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrder(id, token);
        return order.OrderStatus;
    }

    public Task<RegionEntity[]> GetRegions(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        return Task.FromResult(Regions.Values.ToArray());
    }

    public Task<OrderEntity[]> GetOrders(
        RegionEntity[] reqRegions,
        OrderTypeEntity orderType,
        PaginationEntity pagination,
        SortOrderEntity sortOrder = SortOrderEntity.Asc,
        ValueOrderEntity valueOrder = ValueOrderEntity.None,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        CheckExistance(reqRegions);

        var reqRegionsIds = reqRegions.Select(r => r.Id);
        var orders = Orders.Where(o => reqRegionsIds.Contains(o.Value.CreatedRegion.Id)
            && o.Value.OrderType == orderType).Select(o => o.Value);
        
        token.ThrowIfCancellationRequested();

        orders = Sort(orders, valueOrder, sortOrder);
        orders = Paginate(orders, pagination);
        return Task.FromResult(orders.ToArray());
    }

    public Task<IReadOnlyCollection<OrderEntity>> GetOrdersByCustomer(
        long customerId,
        DateTime sinceTimestamp,
        PaginationEntity pagination,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); 

        var orders = Orders
           .Where(o => o.Value.CustomerId == customerId && o.Value.Created >= sinceTimestamp)
           .Select(o => o.Value);
        orders = Paginate(orders, pagination);
        IReadOnlyCollection<OrderEntity> ordersCollection = orders.ToList().AsReadOnly();

        return Task.FromResult(ordersCollection);
    }

    public Task<Dictionary<int, OrdersStatisticEntity>> GetAggregatedOrdersByRegion(
        RegionEntity[] reqRegions,
        DateTime sinceTimestamp,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var orders = Orders
            .Where(o => o.Value.Created > sinceTimestamp);
        if (reqRegions.Any())
        {
            CheckExistance(reqRegions);
            var reqRegionsIds = reqRegions.Select(r => r.Id);
            orders = orders.Where(o => reqRegionsIds.Contains(o.Value.CreatedRegion.Id));
        }
        var groupedOrders = orders.GroupBy(o => o.Value.CreatedRegion);
        
        var result = new Dictionary<int, OrdersStatisticEntity>();
        foreach (var group in groupedOrders)
        {
            var region = group.Key;
            var ordersInGroup = group.Select(t => t.Value);

            var ordersCount = ordersInGroup.Count();
            var price = ordersInGroup.Sum(o => o.Price);
            var weight = ordersInGroup.Sum(o => o.Weight);
            var customerCount = ordersInGroup.Select(o => o.CustomerId).Distinct().Count();
            result.Add(region.Id, new OrdersStatisticEntity(region.Name, ordersCount, price, weight, customerCount));
        }

        return Task.FromResult(result);
    }

    private static Task<OrderEntity> FindOrder(
        long id,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        return Orders.TryGetValue(id, out var order)
            ? Task.FromResult(order)
            : Task.FromException<OrderEntity>(new NotFoundException($"Order with id={id} not found"));
    }

    private static bool CheckExistance(
        RegionEntity[] reqRegions)
    {
        bool isSubset = reqRegions.All(elem => Regions.ContainsKey(elem.Id));
        if (!isSubset)
        {
            throw new NotFoundException("At least one region from the request is not presented in the service.");
        }
        return isSubset;
    }

    private static IEnumerable<OrderEntity> Paginate(
        IEnumerable<OrderEntity> list, 
        PaginationEntity pagination)
    {
        return list.Skip(pagination.Offset).Take(pagination.Limit);
    }

    private static IEnumerable<OrderEntity> Sort(IEnumerable<OrderEntity> orders, 
        ValueOrderEntity valueOrder, SortOrderEntity sortOrder) => valueOrder switch
        {
            ValueOrderEntity.None => orders,
            ValueOrderEntity.Region =>
                sortOrder == SortOrderEntity.Asc
                ? orders.OrderBy(o => o.CreatedRegion.Name)
                : orders.OrderByDescending(o => o.CreatedRegion.Name),
            ValueOrderEntity.Status =>
                sortOrder == SortOrderEntity.Asc
                ? orders.OrderBy(o => o.OrderStatus.GetDisplayName())
                : orders.OrderByDescending(o => o.OrderStatus.GetDisplayName()),

            _ => throw new ArgumentOutOfRangeException(nameof(valueOrder), valueOrder, null)
        };
}