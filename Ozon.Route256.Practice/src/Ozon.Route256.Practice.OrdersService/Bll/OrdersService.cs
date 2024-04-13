using System.Transactions;
using Grpc.Core;
using NpgsqlTypes;
using Ozon.Route256.Practice.OrdersService.CachedClients;
using Ozon.Route256.Practice.OrdersService.Dal.Models;
using Ozon.Route256.Practice.OrdersService.Dal.Repositories;
using Ozon.Route256.Practice.OrdersService.DataAccess.Entities;
using Ozon.Route256.Practice.OrdersService.Exceptions;
using Ozon.Route256.Practice.OrdersService.GrpcClients;

namespace Ozon.Route256.Practice.OrdersService.Bll;

public class OrdersService : IOrdersService
{
    private static readonly HashSet<OrderStatusEntity> _forbiddenToCancelStatus = new()
    {
        OrderStatusEntity.Cancelled,
        OrderStatusEntity.Delivered
    };
    private readonly CachedCustomersClient _cachedCustomersClient;
    private readonly LogisticsSimulatorClient _logisticsService;
    private readonly IRegionsRepositoryPg _regionsRepository;
    private readonly IOrdersRepositoryPg _ordersRepository;
    private readonly IAddressesRepositoryPg _addressesRepository;

    public OrdersService(
        CachedCustomersClient cachedCustomersClients,
        LogisticsSimulatorClient logisticsService,
        IRegionsRepositoryPg regionsRepository,
        IOrdersRepositoryPg ordersRepository,
        IAddressesRepositoryPg addressesRepository)
    {
        _logisticsService = logisticsService;
        _cachedCustomersClient = cachedCustomersClients;
        _ordersRepository = ordersRepository;
        _regionsRepository = regionsRepository;
        _addressesRepository = addressesRepository;
    }

    public async Task AddOrderAsync(
        OrderEntity order,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        //using var transactionScope = new TransactionScope( // todo
        //    TransactionScopeOption.Required,
        //    new TransactionOptions
        //    {
        //        IsolationLevel = IsolationLevel.ReadUncommitted,
        //        Timeout = TimeSpan.FromSeconds(60)
        //    },
        //    TransactionScopeAsyncFlowOption.Enabled);

        var addressId = (await _addressesRepository.CreateAsync(new[] { From(order.Id, order.DeliveryAddress) }, token))
            .First();

        await _ordersRepository.CreateAsync(From(order, addressId), token);

        //transactionScope.Complete();
    }

    public async Task CancelOrderAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        await ThrowIfCancelProhibitedAsync(id, token, internalRequest);

        var cancelResult = await _logisticsService.CancelOrderAsync(id);
        if (cancelResult is null || cancelResult.Success)
        {
            throw new UnprocessableException($"Cannot cancel order with id={id}");
        }
        await _ordersRepository.UpdateStatusAsync(id , OrderStatus.Cancelled, token);
    }

    public async Task UpdateOrderStatusAsync(
        long id,
        OrderStatusEntity status,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _ordersRepository.UpdateStatusAsync(id, From(status), token);
    }

    public async Task<OrderStatusEntity> GetOrderStatusAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrderAsync(id, token, internalRequest);
        return order.OrderStatus;
    }

    public async Task<RegionEntity[]> GetRegionsAsync(
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        RegionDal[] regionsDal = await _regionsRepository.GetAllAsync(token);
        return regionsDal.Select(r => From(r)).ToArray();
    }

    public async Task<OrderEntity[]> GetOrdersAsync(
        RegionEntity[] reqRegions,
        OrderTypeEntity orderType,
        PaginationEntity pagination,
        CancellationToken token,
        SortOrderEntity sortOrder = SortOrderEntity.Asc,
        ValueOrderEntity valueOrder = ValueOrderEntity.None,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        IEnumerable<string> reqRegionsNames = reqRegions.Select(r => r.Name);
        var regionsDal = await _regionsRepository.FindManyAsync(
            reqRegionsNames, token);
        var regions = regionsDal.Select(r => From(r));
        EnsureExistance(reqRegions, regions);

        var filterOptions = new OrderFilterOptions
        {
            Limit = pagination.Limit,
            Offset = pagination.Offset,
            ReqRegionsNames = reqRegionsNames,
            FilterOrderType = true,
            Type = From(orderType),
            SortColumn = From(valueOrder),
            AscSort = sortOrder == SortOrderEntity.Asc
        };

        var ordersInfo = await _ordersRepository.GetAllAsync(filterOptions, token, internalRequest);
        var orders = ordersInfo.Select(o => From((o.order, o.address, o.region)));
        return orders.ToArray();
    }

    public async Task<OrderEntityBase> FindOrderAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var orderDal = await _ordersRepository.FindAsync(id, token, internalRequest);
        return orderDal is null
            ? throw new NotFoundException($"Order with id={id} not found")
            : From(orderDal);
    }

    public async Task<bool> IsOrderExistAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var orderDal = await _ordersRepository.FindAsync(id, token, internalRequest);
        return orderDal is not null;
    }

    public async Task<IReadOnlyCollection<OrderEntity>> GetOrdersByCustomerAsync(
        int customerId,
        DateTime sinceTimestamp,
        PaginationEntity pagination,
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
            Limit = pagination.Limit,
            Offset = pagination.Offset,
            CustomerId = customerId,
            SinceTimestamp = sinceTimestamp,
        };
        var ordersInfo = await _ordersRepository.GetAllAsync(filterOptions, token, internalRequest);
        var orders = ordersInfo.Select(o => From((o.order, o.address, o.region)));
        return orders.ToArray();
    }

    public async Task<List<OrdersStatisticEntity>> GetAggregatedOrdersByRegionAsync(
        RegionEntity[] reqRegions,
        DateTime sinceTimestamp,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        IEnumerable<string> reqRegionsNames = reqRegions.Select(r => r.Name);
        var regionsDal = await _regionsRepository.FindManyAsync(
            reqRegionsNames, token);
        var regions = regionsDal.Select(r => From(r));
        EnsureExistance(reqRegions, regions);

        var filterOptions = new OrderFilterOptions
        {
            ReqRegionsNames = reqRegionsNames,
            SinceTimestamp = sinceTimestamp
        };
        var ordersInfo = await _ordersRepository.GetAllAsync(filterOptions, token, internalRequest);
        var orders = ordersInfo.Select(o => From((o.order, o.address, o.region)));

        var groupedOrders = orders.GroupBy(o => o.CreatedRegion.Name);
        var result = new List<OrdersStatisticEntity>();
        foreach (var group in groupedOrders)
        {
            var region = group.Key;

            var ordersCount = group.Count();
            var price = group.Sum(o => o.Price);
            var weight = group.Sum(o => o.Weight);
            var customerCount = group.Select(o => o.CustomerId).Distinct().Count();
            result.Add(new OrdersStatisticEntity(region, ordersCount, price, weight, customerCount));
        }

        return result;
    }

    private async Task<OrderEntityBase> ThrowIfCancelProhibitedAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var order = await FindOrderAsync(id, token, internalRequest);
        if (_forbiddenToCancelStatus.Contains(order.OrderStatus))
        {
            throw new UnprocessableException($"Cannot cancel order with id={id} in state {order.OrderStatus}.");
        }
        return order;
    }

    private static void EnsureExistance(
        RegionEntity[] reqRegions,
        IEnumerable<RegionEntity> regions)
    {
        if (reqRegions.Length != regions.Count())
        {
            throw new NotFoundException("At least one region from the request is not presented in the service.");
        }
    }

    private static RegionEntity From(RegionDal region) => new(
        region.Name,
        (region.DepotLatLon.X, region.DepotLatLon.Y));

    private static OrderEntity From((OrderDal order, AddressDal address, RegionDal region) from)
        => new(
            Id: from.order.Id,
            OrderStatus: From(from.order.OrderStatus),
            OrderType: From(from.order.OrderType),
            CustomerId: from.order.CustomerId,
            CustomerFullName: from.order.CustomerFullName,
            CustomerMobileNumber: from.order.CustomerMobileNumber,
            DeliveryAddress: From(from.address),
            ItemsCount: from.order.ItemsCount,
            Price: from.order.Price,
            Weight: from.order.Weight,
            Created: from.order.Created,
            CreatedRegion: From(from.region));

    private static OrderEntityBase From(OrderDal order) => new(
        Id: order.Id,
        OrderStatus: From(order.OrderStatus),
        OrderType: From(order.OrderType),
        CustomerId: order.CustomerId,
        CustomerFullName: order.CustomerFullName,
        CustomerMobileNumber: order.CustomerMobileNumber,
        AddressId: order.AddressId,
        ItemsCount: order.ItemsCount,
        Price: order.Price,
        Weight: order.Weight,
        Created: order.Created,
        RegionName: order.RegionName);

    private static OrderDal From(OrderEntity order, int addressId) => new(
        Id: order.Id,
        OrderStatus: From(order.OrderStatus),
        OrderType: From(order.OrderType),
        CustomerId: order.CustomerId,
        CustomerFullName: order.CustomerFullName,
        CustomerMobileNumber: order.CustomerMobileNumber,
        AddressId: addressId,
        ItemsCount: order.ItemsCount,
        Price: order.Price,
        Weight: order.Weight,
        Created: order.Created,
        RegionName: order.CreatedRegion.Name);

    private static AddressEntity From(AddressDal address) => new(
        address.RegionName,
        address.City,
        address.Street,
        address.Building,
        address.Apartment,
        Latitude: address.CoordinateLatLon.X,
        Longitude: address.CoordinateLatLon.Y);

    private static AddressDalToInsert From(long orderId, AddressEntity address) => new(
        RegionName: address.Region,
        City: address.City,
        Street: address.Street,
        Building: address.Building,
        Apartment: address.Apartment,
        CoordinateLatLon: new NpgsqlPoint(address.Latitude, address.Longitude),
        OrderId: orderId);

    private static OrderStatusEntity From(OrderStatus orderStatus) =>
        orderStatus switch
        {
            OrderStatus.Created => OrderStatusEntity.Created,
            OrderStatus.SentToCustomer => OrderStatusEntity.SentToCustomer,
            OrderStatus.Lost => OrderStatusEntity.Lost,
            OrderStatus.Delivered => OrderStatusEntity.Delivered,
            OrderStatus.Cancelled => OrderStatusEntity.Cancelled,
            OrderStatus.PreOrder => OrderStatusEntity.PreOrder,

            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };

    private static OrderStatus From(OrderStatusEntity orderStatus) =>
        orderStatus switch
        {
            OrderStatusEntity.Created => OrderStatus.Created,
            OrderStatusEntity.SentToCustomer => OrderStatus.SentToCustomer,
            OrderStatusEntity.Lost => OrderStatus.Lost,
            OrderStatusEntity.Delivered => OrderStatus.Delivered,
            OrderStatusEntity.Cancelled => OrderStatus.Cancelled,
            OrderStatusEntity.PreOrder => OrderStatus.PreOrder,

            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };

    private static OrderTypeEntity From(OrderType orderType) => orderType switch
    {
        OrderType.Api => OrderTypeEntity.Api,
        OrderType.Mobile => OrderTypeEntity.Mobile,
        OrderType.Web => OrderTypeEntity.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };

    private static OrderType From(OrderTypeEntity orderType) => orderType switch
    {
        OrderTypeEntity.Api => OrderType.Api,
        OrderTypeEntity.Mobile => OrderType.Mobile,
        OrderTypeEntity.Web => OrderType.Web,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };

    private static ValueOrderDal From(ValueOrderEntity orderType) => orderType switch
    {
        ValueOrderEntity.None => ValueOrderDal.None,
        ValueOrderEntity.Region => ValueOrderDal.Region,
        ValueOrderEntity.Status => ValueOrderDal.Status,

        _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null)
    };
}