using System;
using FluentMigrator;

using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(1, "Initial schema")]
public sealed class InitialSchema : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
CREATE TABLE IF NOT EXISTS item_inventory (
       item_id       bigint    primary key 
     , reserved      int       not null default 0
     , sold          int       not null default 0
     , cancelled     int       not null default 0
     , last_updated  timestamp with time zone not null
);

CREATE TABLE IF NOT EXISTS seller_sales (
       seller_id     bigint    primary key 
     , amount        numeric   not null default 0.0
     , currency      text      not null
     , quantity      int       not null default 0
     , last_updated  timestamp with time zone not null
);";
}
