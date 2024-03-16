using static Ozon.Route256.Practice.Replica.Types;

namespace Ozon.Route256.Practice.OrdersService.ClientBalancing
{
    public sealed record DbEndpoint(
        string HostAndPort,
        DbReplicaType DbReplica,
        int[] Buckets);
}
