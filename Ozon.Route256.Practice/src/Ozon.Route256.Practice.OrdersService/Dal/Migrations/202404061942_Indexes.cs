using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;

[Migration(3, "Indexes")]
public class Indexes : ShardSqlMigration
{
    protected override string GetUpSql(
            IServiceProvider services) => @"
create index idx_type_region_status_id --todo после шаридрования?
on orders (type, region_name, status, id);

create index idx_customer_created_id
on orders (customer_id, created, id);

create index idx_region_created_id
on orders(region_name, created, id)
";

    protected override string GetDownSql(
        IServiceProvider services) => @"
drop index idx_type_region_status_id, idx_customer_created_id, idx_region_created_id
";
}
