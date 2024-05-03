using Grpc.Core;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Practice.OrdersService.Application.Exceptions;
using Ozon.Route256.Practice.OrdersService.Application.Helpers;
using Ozon.Route256.Practice.OrdersService.Application.Infrastructure.Metrics;
using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Application.Bll;

public class OrdersService : IOrdersService
{
    private readonly ICachedCustomersClient _cachedCustomersClient;
    private readonly ILogisticsSimulatorServiceClient _logisticsService;
    private readonly IOrdersAggregateRepository _ordersRepository;
    private readonly IOrdersMetrics _metrics;
    private readonly ILogger<OrdersService> _logger;

    public OrdersService(
        ICachedCustomersClient cachedCustomersClients,
        ILogisticsSimulatorServiceClient logisticsService,
        IOrdersAggregateRepository ordersRepository,
        IOrdersMetrics metrics,
        ILogger<OrdersService> logger)
    {
        _logisticsService = logisticsService;
        _cachedCustomersClient = cachedCustomersClients;
        _ordersRepository = ordersRepository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task AddOrderAsync(
        OrderAggregate order,
        CancellationToken token)
    {
        await _ordersRepository.AddAsync(order, token);
    }

    public async Task CancelOrderAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        var order = await ThrowIfCancelProhibitedAsync(id, token, internalRequest);
        using (var mapperActivity = Diagnostics.ActivitySource.StartActivity(Diagnostics.LogisticsServiceCancellation))
        {
            var (success, error) = await _logisticsService.CancelOrderAsync(id);
            mapperActivity?.SetTag("Logistics result", success);
            if (!success)
            {
                _metrics.CancellationTried(false);
                throw new UnprocessableException($"Cannot cancel order with id={id}. Reason: {error ?? "error is empty"}");
            }
        }

        using (var mapperActivity = Diagnostics.ActivitySource.StartActivity(Diagnostics.DbStatusUpdate))
        {
            _metrics.OrderCancelled(order.OrderStatus);
            _metrics.CancellationTried(true);
            await _ordersRepository.UpdateOrderStatusAsync(id, OrderStatusModel.Cancelled, token);
        }
    }

    public async Task HandleNewStatusAsync(
        long id,
        OrderStatusModel status,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _ordersRepository.UpdateOrderStatusAsync(id, status, token);
    }

    public async Task<OrderStatusModel> GetOrderStatusAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrderAsync(id, token, internalRequest);
        return order.OrderStatus;
    }

    public async Task<Region[]> GetRegionsAsync(
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _ordersRepository.GetRegionsAsync(token);
    }

    public async Task<OrderAggregate[]> GetOrdersAsync(
        Region[] reqRegions,
        OrderTypeModel orderType,
        Pagination pagination,
        CancellationToken token,
        SortOrderDto sortOrder = SortOrderDto.Asc,
        ValueOrderDto valueOrder = ValueOrderDto.None,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        IEnumerable<string> reqRegionsNames = reqRegions.Select(r => r.Name);
        var regions= await _ordersRepository.FindRegionsAsync(reqRegionsNames, token);
        EnsureExistance(reqRegions, regions);

        var filterOptions = new OrderFilterOptions
        {
            ReqRegionsNames = reqRegionsNames,
            FilterOrderType = true,
            OrderType = orderType,
        };

        var orders = await _ordersRepository.GetOrdersAsync(filterOptions, token, internalRequest);
        orders = Sort(orders, valueOrder, sortOrder == SortOrderDto.Asc);
        orders = Paginate(orders, pagination);
        return orders.ToArray();
    }

    public async Task<Order> FindOrderAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await _ordersRepository.FindOrderAsync(id, token, internalRequest);
        return order is null
            ? throw new NotFoundException($"Order with id={id} not found")
            : order;
    }

    public async Task<bool> IsOrderExistAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await _ordersRepository.FindOrderAsync(id, token, internalRequest);
        return order is not null;
    }

    public async Task<IReadOnlyCollection<OrderAggregate>> GetOrdersByCustomerAsync(
        int customerId,
        DateTime sinceTimestamp,
        Pagination pagination,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            await _cachedCustomersClient.EnsureExistsAsync(customerId, token);
        }
        catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
        {
            throw new InvalidArgumentException(ex.Status.Detail);
        }

        token.ThrowIfCancellationRequested();

        var filterOptions = new OrderFilterOptions
        {
            CustomerId = customerId,
            SinceTimestamp = sinceTimestamp,
        };
        var orders = await _ordersRepository.GetOrdersAsync(filterOptions, token, internalRequest);
        orders = Paginate(orders, pagination);
        return orders.ToArray();
    }

    public async Task<List<OrdersStatisticEntity>> GetAggregatedOrdersByRegionAsync(
        Region[] reqRegions,
        DateTime sinceTimestamp,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        IEnumerable<string> reqRegionsNames = reqRegions.Select(r => r.Name);
        var regions = await _ordersRepository.FindRegionsAsync(
            reqRegionsNames, token);
        EnsureExistance(reqRegions, regions);

        var filterOptions = new OrderFilterOptions
        {
            ReqRegionsNames = reqRegionsNames,
            SinceTimestamp = sinceTimestamp
        };
        var orders = await _ordersRepository.GetOrdersAsync(filterOptions, token, internalRequest);

        var groupedOrders = orders.GroupBy(o => o.CreatedRegion.Name);
        var result = new List<OrdersStatisticEntity>();
        foreach (var group in groupedOrders)
        {
            var region = group.Key;

            var ordersCount = group.Count();
            var price = group.Sum(o => o.Order.Price);
            var weight = group.Sum(o => o.Order.Weight);
            var customerCount = group.Select(o => o.Order.CustomerId).Distinct().Count();
            result.Add(new OrdersStatisticEntity(region, ordersCount, price, weight, customerCount));
        }

        return result;
    }

    private async Task<Order> ThrowIfCancelProhibitedAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrderAsync(id, token, internalRequest);
        if (!order.CanCancel())
        {
            _metrics.CancellationTried(false);
            throw new UnprocessableException($"Cannot cancel order with id={id} in state {order.OrderStatus}.");
        }
        return order;
    }

    private static void EnsureExistance(
        Region[] reqRegions,
        IEnumerable<Region> regions)
    {
        if (reqRegions.Length != regions.Count())
        {
            throw new NotFoundException("At least one region from the request is not presented in the service.");
        }
    }

    private static IEnumerable<OrderAggregate> Sort(
        IEnumerable<OrderAggregate> orders,
        ValueOrderDto valueOrder,
        bool AscSort) => valueOrder switch
    {
        ValueOrderDto.None => orders,
        ValueOrderDto.Region =>
                AscSort
                ? orders.OrderBy(o => o.CreatedRegion.Name)
                : orders.OrderByDescending(o => o.CreatedRegion.Name),
        ValueOrderDto.Status =>
                AscSort
                ? orders.OrderBy(o => o.Order.OrderStatus.ToString())
                : orders.OrderByDescending(o => o.Order.OrderStatus.ToString()),

        _ => throw new ArgumentOutOfRangeException(nameof(valueOrder), valueOrder, null)
    };

    private static IEnumerable<OrderAggregate> Paginate(
        IEnumerable<OrderAggregate> list,
        Pagination pagination) 
            => list.Skip(pagination.Offset).Take(pagination.Limit);
}