using System;
using FluentMigrator;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(3, "Add seller_sale_v1 pg_type")]
public sealed class AddSellerSaleV1Type : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'seller_sale_v1') THEN
            CREATE TYPE seller_sale_v1 AS 
            (
                  seller_id     bigint
                , amount        numeric
                , currency      text
                , quantity      int
                , last_updated  timestamp with time zone
            );
        END IF;
    END
$$;";
}