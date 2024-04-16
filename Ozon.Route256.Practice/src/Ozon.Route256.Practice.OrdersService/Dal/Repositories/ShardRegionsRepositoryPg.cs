using Npgsql;
using NpgsqlTypes;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public class ShardRegionsRepositoryPg : BaseShardRepository, IRegionsRepositoryPg
{
    private const string Table = $"{ShardsHelper.BucketPlaceholder}.regions";
    private const string Fields = "name, depot_lat_lon";

    public ShardRegionsRepositoryPg(
        IShardPostgresConnectionFactory connectionFactory,
        IShardingRule<long> shardingRule)
            : base(connectionFactory, shardingRule)
    {
    }

    public async Task<RegionDal[]> GetAllAsync(
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); 

        const string sql = @$"
            select {Fields}
            from {Table};
        ";
        await using var connection = await OpenConnectionByBucket(AllBuckets.First(), token);

        var command = connection.NpgsqlConnection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await connection.ExecuteReaderAsync(command, token);
        var result = await ReadRegionsDalAsync(reader, token);
        return result.ToArray();
    }

    public async Task<RegionDal[]> FindManyAsync(
        IEnumerable<string> names,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        const string sql = @$"
            select {Fields}
            from {Table}
            where name = any(:names::text[]);
        ";

        await using var connection = await OpenConnectionByBucket(AllBuckets.First(), token);
        var command = connection.NpgsqlConnection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("names", names.ToArray());

        await using var reader = await connection.ExecuteReaderAsync(command, token);
        var result = await ReadRegionsDalAsync(reader, token);
        return result.ToArray();
    }

    private static async Task<RegionDal[]> ReadRegionsDalAsync(
        NpgsqlDataReader reader,
        CancellationToken token)
    {
        var result = new List<RegionDal>();
        while (await reader.ReadAsync(token))
        {
            result.Add(
                new RegionDal(
                    Name: reader.GetFieldValue<string>(0),
                    DepotLatLon: reader.GetFieldValue<NpgsqlPoint>(1)
                ));
        }

        return result.ToArray();
    }
}