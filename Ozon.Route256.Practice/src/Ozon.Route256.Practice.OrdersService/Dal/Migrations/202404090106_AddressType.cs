using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common.Shard;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;

[Migration(4, "Address type")]
public class AddressType : ShardSqlMigration
{
    protected override string GetUpSql(
        IServiceProvider services) => @"

do $$
begin
    if not exists (select 1 from pg_type where typname = 'address_dal_to_insert') then        
        create type public.address_dal_to_insert as 
        (
            region_name text,
            city text,
            street text,
            building text,
            apartment text,
            coordinate_lat_lon point,
            order_id bigint
        );
    end if;

end $$;
";

    protected override string GetDownSql(
        IServiceProvider services) => @"

drop type address_dal_to_insert;
";
}