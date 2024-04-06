using System.Data;
using Npgsql;
using NpgsqlTypes;
using Ozon.Route256.Practice.OrdersService.Dal.Common;
using Ozon.Route256.Practice.OrdersService.Dal.Models;

namespace Ozon.Route256.Practice.OrdersService.Dal.Repositories;

public class RegionsRepositoryPg : IRegionsRepositoryPg
{
    private const string Fields = "name, depot_lat_lon";
    private const string Table = "regions";

    private readonly IPostgresConnectionFactory _connectionFactory;

    public RegionsRepositoryPg(
        IPostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RegionDal[]> GetAllAsync(
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); 

        const string sql = @$"
            select {Fields}
            from {Table};
        ";
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(token);

        var result = await ReadRegionsDalAsync(reader, token);
        return result.ToArray();
    }

    public async Task<RegionDal?> FindAsync(
        string name,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        const string sql = @$"
            select {Fields}
            from {Table}
            where name = :name;
        ";

        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("name", name);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, token);

        var result = await ReadRegionsDalAsync(reader, token);
        return result.FirstOrDefault();
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

        await using var connection = await _connectionFactory.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("names", names.ToArray());
        await using var reader = await command.ExecuteReaderAsync(token);

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