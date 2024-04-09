using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations;

[Migration(1, "Initial migration")]
public class Initial : SqlMigration
{
    protected override string GetUpSql(
        IServiceProvider services) => @"

create table addresses(
    id serial primary key,
    region_name text not null, 
    city text not null,
    street text not null,
    building text not null,
    apartment text not null,
    coordinate_lat_lon point not null
);

create table regions(
    name text not null primary key,
    depot_lat_lon point not null
);

create type order_status as enum ('created', 'sent_to_customer', 'delivered', 'lost', 'cancelled', 'pre_order');

create type order_type as enum ('web', 'api', 'mobile');

create table orders(
    id bigint primary key,
    status order_status not null,
    type order_type not null,
    customer_id integer not null,
    customer_full_name text not null,
    customer_mobile_number text not null,
    address_id integer not null,
    items_count integer not null, 
    price decimal not null, 
    weight numeric not null,
    created timestamp with time zone not null,
    region_name text not null,

    constraint fk_address
        FOREIGN KEY(address_id) 
        REFERENCES addresses(id),

    constraint fk_region
        FOREIGN KEY(region_name) 
        REFERENCES regions(name)
);
";

    protected override string GetDownSql(
        IServiceProvider services) => @"
drop table addresses cascade;
drop table regions cascade;
drop table orders cascade;
drop type order_status cascade;
drop type order_type cascade;
";
}