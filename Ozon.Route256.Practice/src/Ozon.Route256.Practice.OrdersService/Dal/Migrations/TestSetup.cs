using FluentMigrator;
using Ozon.Route256.Practice.OrdersService.Dal.Common;

namespace Ozon.Route256.Practice.OrdersService.Dal.Migrations
{
    [Migration(2, "Test Setup")]

    public class TestSetup : SqlMigration
    {
        protected override string GetUpSql(
            IServiceProvider services) => @"
            
            insert into addresses (region_name, city, street, building, apartment, coordinate_lat_lon)
            values
                ('spb', 'spb', 'nevskaya', 1, 1, point(75, 54)),
                ('msk', 'msk', 'tverskaya', 1, 1, point(75, 54)),
                ('nsk', 'nsk', 'akademgorodok..', 1, 1, point(75, 54));
            
            insert into regions (name, depot_lat_lon)
            values
                ('spb', point(75, 54)),
                ('msk', point(75, 54)),
                ('nsk', point(75, 54));
            
            insert into orders(id, status, type, customer_id, customer_full_name, customer_mobile_number, address_id, items_count, price, weight, created, region_name)
            values
	            (1, 'created', 'api', 111, 'Oksana', '8977', 1, 10, 999, 1000, current_timestamp, 'spb'),
	            (2, 'created', 'mobile', 111, 'Oksana', '8977', 1, 10, 999, 1000, current_timestamp, 'spb'),
	            (3, 'created', 'api', 112, 'Ok', '8977', 2, 2000, 99999, 1, current_timestamp, 'msk'),
	            (4, 'created', 'api', 112, 'Ok', '8977', 2, 2000, 99999, 1, current_timestamp, 'msk'),	
	            (5, 'created', 'api', 112, 'Ok', '8977', 2, 2000, 99999, 1, current_timestamp, 'msk')
            ;";

            
        protected override string GetDownSql(
            IServiceProvider services) => "";
    }
}
