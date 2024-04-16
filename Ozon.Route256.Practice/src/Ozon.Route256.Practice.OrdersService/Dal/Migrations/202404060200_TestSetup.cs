using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;

[Migration(2, "Test Setup")]

public class TestSetup : ShardSqlMigration
{
    protected override string GetUpSql(
        IServiceProvider services) => @"

            insert into regions (name, depot_lat_lon)
            values
                ('spb', point(75, 54)),
                ('msk', point(75, 54)),
                ('nsk', point(75, 54));

            ;";

    protected override string GetDownSql(
        IServiceProvider services) => "";
}
