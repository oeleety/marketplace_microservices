using Npgsql;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public class AddressesRepositoryPg : IAddressesRepositoryPg
{
    private const string FieldsForInsert = "region_name, city, street, building, apartment, coordinate_lat_lon";
    private const string Fields = "id, " + FieldsForInsert;
    private const string Table = "addresses";

    private readonly IPostgresConnectionFactory _connectionFactory;

    public AddressesRepositoryPg(
        IPostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int[]> CreateAsync(
        AddressDalToInsert[] addresses,
        CancellationToken token)
    {
        const string sql = @$"
            insert into {Table} ({FieldsForInsert})
            select {FieldsForInsert} from unnest(:models)
            returning id;
        ";

        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("models", addresses);

        var reader = await command.ExecuteReaderAsync(token);
        var result = await ReadIdsAsync(reader, token);
        return result;
    }

    private static async Task<int[]> ReadIdsAsync(
        NpgsqlDataReader reader,
        CancellationToken token)
    {
        var result = new List<int>();
        while (await reader.ReadAsync(token))
        {
            result.Add(reader.GetFieldValue<int>(0));
        }
        return result.ToArray();
    }
}