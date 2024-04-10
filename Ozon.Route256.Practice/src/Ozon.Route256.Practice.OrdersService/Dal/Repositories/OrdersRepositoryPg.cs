using System.Data;
using Npgsql;
using NpgsqlTypes;
using Ozon.Route256.Practice.OrdersService.Bll;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public sealed class OrdersRepositoryPg : IOrdersRepositoryPg
{
    private const string Table = "orders";
    private const string Fields = "id, status, type, customer_id, customer_full_name, customer_mobile_number, " +
        "address_id, items_count, price, weight, created, region_name";
    private const string FieldsWithTableAliasO = "o.id, o.status, type, o.customer_id, o.customer_full_name, o.customer_mobile_number, " +
    "o.address_id, o.items_count, o.price, o.weight, o.created, o.region_name";

    private readonly IPostgresConnectionFactory _connectionFactory;

    public OrdersRepositoryPg(
        IPostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(
        OrderDal order,
        CancellationToken token)
    {
        const string sql = @$"
        insert into {Table} ({Fields})
        values (:id, :status, :type, :customer_id, :customer_full_name, :customer_mobile_number, :address_id, 
            :items_count, :price, :weight, :created, :region_name);
        ";

        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add("id", order.Id);
        command.Parameters.Add("status", order.OrderStatus);
        command.Parameters.Add("type", order.OrderType);
        command.Parameters.Add("customer_id", order.CustomerId);
        command.Parameters.Add("customer_full_name", order.CustomerFullName);
        command.Parameters.Add("customer_mobile_number", order.CustomerMobileNumber);
        command.Parameters.Add("address_id", order.AddressId);
        command.Parameters.Add("items_count", order.ItemsCount);
        command.Parameters.Add("price", order.Price);
        command.Parameters.Add("weight", order.Weight);
        command.Parameters.Add("created", order.Created);
        command.Parameters.Add("region_name", order.RegionName);

        await command.ExecuteNonQueryAsync(token);
    }

    public async Task<OrderDal?> FindAsync(
        long id, 
        CancellationToken token,
        bool internalRequest = false)
    {
        string sql = @$"
            select {Fields}
            from {Table}
            where id = :id
        ";
        if (!internalRequest)
        {
            sql += " and status != :status ";
        }
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("id", id);
        command.Parameters.Add("status", OrderStatus.PreOrder);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);
        var result = await ReadOrdersAsync(reader, token);
        return result.FirstOrDefault();
    }

    public async Task UpdateStatusAsync(
        IEnumerable<long> ids,
        OrderStatus status,
        CancellationToken token)
    {
        const string sql = @$"
            update {Table}
            set status = :status
            where id = any(:ids::bigint[]);
        ";

        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("status", status);
        command.Parameters.Add("ids", ids.ToArray());

        await command.ExecuteNonQueryAsync(token);
    }

    public async Task<List<(OrderDal order, AddressDal address, RegionDal region)>> GetAllAsync(
        OrderFilterOptions filterOptions,
        CancellationToken token,
        bool internalRequest = false)
    {
        token.ThrowIfCancellationRequested();

        string sql = @$"
            select {FieldsWithTableAliasO}, r.name, 
            a.id, a.region_name, a.city, a.street, a.building, a.apartment, a.coordinate_lat_lon
            from {Table} o
            inner join regions r on o.region_name = r.name
            inner join addresses a on o.address_id = a.id
            where 1=1 
        ";
        if (!internalRequest)
        {
            sql += " and status != :status ";
        }
        if (filterOptions.ReqRegionsNames.Any())
        {
            sql += "and o.region_name = any(:names::text[]) ";
        }
        if (filterOptions.CustomerId != default)
        {
            sql += "and customer_id = :customer_id ";
        }
        if (filterOptions.FilterOrderType)
        {
            sql += "and type = :type ";
        }
        if (filterOptions.SinceTimestamp != default)
        {
            sql += "and created >= :since ";
        }
        if (filterOptions.SortColumn != default)
        {
            var sort = filterOptions.AscSort ? "" : "desc ";
            var sortColumn = filterOptions.SortColumn;
            var orderByString = sortColumn switch
            {
                ValueOrderDal.None => "",
                ValueOrderDal.Region => $"order by o.region_name {sort}, o.id ",
                ValueOrderDal.Status => $"order by o.status {sort}, o.id ",

                _ => throw new ArgumentOutOfRangeException(nameof(sortColumn), sortColumn, null)
            };
            sql += orderByString;
        }
        else
        {
            sql += "order by o.id ";
        }
        if (filterOptions.Limit != -1 && filterOptions.Offset != -1)
        {
            sql += "limit :limit offset :offset ";
        }
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("limit", filterOptions.Limit);
        command.Parameters.Add("offset", filterOptions.Offset);
        command.Parameters.Add("names", filterOptions.ReqRegionsNames.ToArray());
        command.Parameters.Add("customer_id", filterOptions.CustomerId);
        command.Parameters.Add("type", filterOptions.Type);
        command.Parameters.Add("since", filterOptions.SinceTimestamp);
        command.Parameters.Add("status", OrderStatus.PreOrder);

        await using var reader = await command.ExecuteReaderAsync(token);

        var result = await ReadOrdersFullAsync(reader, token);
        return result;
    }

    private static async Task<OrderDal[]> ReadOrdersAsync(
        NpgsqlDataReader reader,
        CancellationToken token)
    {
        var result = new List<OrderDal>();
        while (await reader.ReadAsync(token))
        {
            var order = ReadOrder(reader);
            result.Add(order);
        }

        return result.ToArray();
    }

    private static async Task<List<(OrderDal order, AddressDal address, RegionDal region)>> ReadOrdersFullAsync(
        NpgsqlDataReader reader,
        CancellationToken token)
    {
        var result = new List<(OrderDal order, AddressDal address, RegionDal region)>();
        while (await reader.ReadAsync(token))
        {
            var order = ReadOrder(reader);
            var region = new RegionDal(
                 reader.GetFieldValue<string>(12),
                 default);
            var address = new AddressDal(
                Id: reader.GetFieldValue<int>(13),
                RegionName: reader.GetFieldValue<string>(14),
                City: reader.GetFieldValue<string>(15),
                Street: reader.GetFieldValue<string>(16),
                Building: reader.GetFieldValue<string>(17),
                Apartment: reader.GetFieldValue<string>(18),
                CoordinateLatLon: reader.GetFieldValue<NpgsqlPoint>(19));
            result.Add((order, address, region));
        }

        return result;
    }

    private static OrderDal ReadOrder(NpgsqlDataReader reader) => new(
        Id: reader.GetFieldValue<long>(0),
        OrderStatus: reader.GetFieldValue<OrderStatus>(1),
        OrderType: reader.GetFieldValue<OrderType>(2),
        CustomerId: reader.GetFieldValue<int>(3),
        CustomerFullName: reader.GetFieldValue<string>(4),
        CustomerMobileNumber: reader.GetFieldValue<string>(5),
        AddressId: reader.GetFieldValue<int>(6),
        ItemsCount: reader.GetFieldValue<int>(7),
        Price: reader.GetFieldValue<decimal>(8),
        Weight: reader.GetFieldValue<float>(9),
        Created: reader.GetFieldValue<DateTime>(10),
        RegionName: reader.GetFieldValue<string>(11));
}