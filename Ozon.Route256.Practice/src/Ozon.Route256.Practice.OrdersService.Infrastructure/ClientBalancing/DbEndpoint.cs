using Npgsql;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.ClientBalancing;

public sealed class DbEndpoint
{
    public DbEndpoint(string hostAndPort, DbReplicaType dbReplica, int[] buckets)
    {
        HostAndPort = hostAndPort;
        DbReplica = dbReplica;
        Buckets = buckets;
    }

    public string HostAndPort { get; private set; }
    public DbReplicaType DbReplica { get; private set; }
    public int[] Buckets { get; private set; }

    public string GetConnectionString(DbOptions dbOptions)
    {
        var hostAndPort = HostAndPort.Split(':');
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = hostAndPort[0],
            Port = int.Parse(hostAndPort[1]),
            Database = dbOptions.DatabaseName,
            Username = dbOptions.User,
            Password = dbOptions.Password
        };
        return builder.ToString();
    }
}
