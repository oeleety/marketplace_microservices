using Npgsql;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public class ShardAddressesRepositoryPg : BaseShardRepository, IAddressesRepositoryPg
{
    private const string FieldsForInsert = "region_name, city, street, building, apartment, coordinate_lat_lon, order_id";
    private const string Fields = "id, " + FieldsForInsert;
    private const string Table = $"{ShardsHelper.BucketPlaceholder}.addresses";

    public ShardAddressesRepositoryPg(
        IShardPostgresConnectionFactory connectionFactory,
        IShardingRule<long> shardingRule)
            : base(connectionFactory, shardingRule)
    {
    }

    public async Task<int[]> CreateAsync(
        AddressDalToInsert[] addresses,
        CancellationToken token)
    {
        var orderId = addresses.First().OrderId;
        if (addresses.Any(x => x.OrderId != orderId))
        {
            throw new ArgumentException("All addresses should have the same order id");
        }

        const string sql = @$"
            insert into {Table} ({FieldsForInsert})
            select {FieldsForInsert} from unnest(:models)
            returning id;
        ";

        await using var connection = await OpenConnectionByShardKeyAsync(orderId, token);
        var command = connection.NpgsqlConnection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("models", addresses);

        var reader = await connection.ExecuteReaderAsync(command, token);
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