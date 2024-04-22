using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Infrastructure.Database.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Infrastructure.Migrations;

[Migration(3, "Indexes")]
public class Indexes : ShardSqlMigration
{
    protected override string GetUpSql(
            IServiceProvider services) => @"
create index idx_type_region_status
on orders (type, region_name, status);

create index idx_customer_created
on orders (customer_id, created);

create index idx_region_created
on orders(region_name, created)
";

    protected override string GetDownSql(
        IServiceProvider services) => @"
drop index idx_type_region_status, idx_customer_created, idx_region_created
";
}