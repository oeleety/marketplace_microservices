using System.Data;
using Npgsql;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

public class ShardNpgsqlConnection
{
    public readonly NpgsqlConnection NpgsqlConnection;

    public ShardNpgsqlConnection(
        NpgsqlConnection npgsqlConnection,
        int bucketId)
    {
        NpgsqlConnection = npgsqlConnection;
        BucketId = bucketId;
    }

    public int BucketId { get; }

    public async Task<NpgsqlDataReader> ExecuteReaderAsync(
        NpgsqlCommand command,
        CancellationToken token, 
        CommandBehavior behavior = CommandBehavior.Default)
    {
        UpdateWithBucketNumber(command);
        return await command.ExecuteReaderAsync(behavior, token);
    }

    public async Task<int> ExecuteNonQueryAsync(
        NpgsqlCommand command,
        CancellationToken token)
    {
        UpdateWithBucketNumber(command);
        return await command.ExecuteNonQueryAsync(token);
    }

    private void UpdateWithBucketNumber(NpgsqlCommand command) =>
         command.CommandText = command.CommandText.Replace(
             ShardsHelper.BucketPlaceholder, 
             ShardsHelper.GetSchemaName(BucketId));

    public  ValueTask DisposeAsync()
    {
        return NpgsqlConnection.DisposeAsync();
    }
}