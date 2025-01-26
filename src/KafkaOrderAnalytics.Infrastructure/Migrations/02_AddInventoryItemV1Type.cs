using System;
using FluentMigrator;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(2, "Add inventory_item_v1 pg_type")]
public sealed class AddInventoryItemV1Type : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'inventory_item_v1') THEN
            CREATE TYPE inventory_item_v1 AS 
            (
                  item_id      bigint
                , reserved     int
                , sold         int
                , cancelled    int
                , last_updated timestamp with time zone
            );
        END IF;
    END
$$;";
}