using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common;

namespace Ozon.Route256.Practice.CustomerService.Dal.Migrations;

[Migration(4, "Address type")]
public class AddressType: SqlMigration
{
    protected override string GetUpSql(
        IServiceProvider services) => @"

create type address_dal_to_insert as
(
    region_name text,
    city text,
    street text,
    building text,
    apartment text,
    coordinate_lat_lon point
);
";

    protected override string GetDownSql(
        IServiceProvider services) =>@"

drop type address;
";
}