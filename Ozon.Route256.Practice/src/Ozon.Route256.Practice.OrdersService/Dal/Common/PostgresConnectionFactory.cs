using Npgsql;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Common;

public interface IPostgresConnectionFactory
{
    Task<NpgsqlConnection> OpenConnectionAsync();
}

public class PostgresConnectionFactory: IPostgresConnectionFactory
{
    private readonly string _connectionString;
    private readonly NpgsqlDataSource _dataSource;

    public PostgresConnectionFactory(
        string connectionString)
    {
        _connectionString = connectionString;
        _dataSource = GetMappedDataSource(_connectionString);
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync() => await _dataSource.OpenConnectionAsync();

    private static NpgsqlDataSource GetMappedDataSource(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<OrderType>("order_type");
        dataSourceBuilder.MapEnum<OrderStatus>("order_status");
        dataSourceBuilder.MapComposite<AddressDalToInsert>("address_dal_to_insert");

        return dataSourceBuilder.Build();
    }
}