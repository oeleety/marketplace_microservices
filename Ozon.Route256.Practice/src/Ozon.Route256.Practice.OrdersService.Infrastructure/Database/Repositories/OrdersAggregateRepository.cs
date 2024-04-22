using System.Transactions;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Dal;
using Ozon.Route256.Practice.OrdersService.Application.Bll;
using Ozon.Route256.Practice.OrdersService.Domain;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Repositories;

internal sealed class OrdersAggregateRepository : IOrdersAggregateRepository
{
    private readonly IRegionsRepository _regionsRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IAddressesRepository _addressesRepository;

    public OrdersAggregateRepository(
        IRegionsRepository regionsRepository,
        IOrdersRepository ordersRepository,
        IAddressesRepository addressesRepository)
    {
        _regionsRepository = regionsRepository;
        _ordersRepository = ordersRepository;
        _addressesRepository = addressesRepository;
    }

    public async Task AddAsync(
        OrderAggregate order,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted,
                Timeout = TimeSpan.FromSeconds(60)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        var addressId = (await _addressesRepository.CreateAsync(new[] { Mappers.From(order.Order.Id, order.Address) }, token))
            .First();

        await _ordersRepository.CreateAsync(Mappers.From(order, addressId), token);

        transactionScope.Complete();
    }

    public async Task UpdateOrderStatusAsync(
        long id,
        OrderStatusModel status,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _ordersRepository.UpdateStatusAsync(id, Mappers.From(status), token);
    }

    public async Task<Region[]> GetRegionsAsync(
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        RegionDal[] regionsDal = await _regionsRepository.GetAllAsync(token);
        return regionsDal.Select(r => Mappers.From(r)).ToArray();
    }

    public async Task<IEnumerable<OrderAggregate>> GetOrdersAsync(
        OrderFilterOptions filterOptions,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var ordersInfo = await _ordersRepository.GetAllAsync(filterOptions, token, internalRequest);
        var orders = ordersInfo.Select(o => Mappers.From((o.order, o.address, o.region)));
        return orders;
    }

    public async Task<IEnumerable<Region>> FindRegionsAsync(
        IEnumerable<string> reqRegionsNames,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var regionsDal = await _regionsRepository.FindManyAsync(
            reqRegionsNames, token);
        var regions = regionsDal.Select(r => Mappers.From(r));
        return regions;
    }

    public async Task<Order> FindOrderAsync(
        long id,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        var orderDal = await _ordersRepository.FindAsync(id, token, internalRequest);
        return orderDal is null ? null : Mappers.From(orderDal);
    }
}
