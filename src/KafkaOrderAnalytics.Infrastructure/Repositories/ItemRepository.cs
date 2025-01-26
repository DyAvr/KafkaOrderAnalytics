using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Repositories;

public sealed class ItemRepository : IItemRepository
{
    private readonly string _connectionString;

    public ItemRepository(string connectionString) => _connectionString = connectionString;

    public async Task UpdateInventory(IEnumerable<InventoryItemEntityV1> inventoryItems, CancellationToken token)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);
        await UpdateInventory(connection, inventoryItems, token);
    }

    private static async Task UpdateInventory(
        NpgsqlConnection connection,
        IEnumerable<InventoryItemEntityV1> inventoryItems,
        CancellationToken token,
        NpgsqlTransaction? transaction = null)
    {
        const string sqlQuery = @"
insert into item_inventory (item_id, reserved, sold, cancelled, last_updated)
select item_id, reserved, sold, cancelled, last_updated
  from UNNEST(@InventoryItems)
 on conflict (item_id) do update
set reserved = item_inventory.reserved + excluded.reserved,
        sold = item_inventory.sold + excluded.sold,
   cancelled = item_inventory.cancelled + excluded.cancelled,
last_updated = excluded.last_updated;";
        
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    InventoryItems = inventoryItems
                },
                transaction: transaction,
                cancellationToken: token));
    }

    public async Task UpdateInventoryAndSales(IEnumerable<InventoryItemEntityV1> inventoryItems, IEnumerable<SellerSaleEntityV1> sellerSales, CancellationToken token)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(token);
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(token);

        try
        {
            await UpdateInventory(connection, inventoryItems, token, transaction);

            const string salesSql = @"
insert into seller_sales (seller_id, amount, currency, quantity, last_updated)
select seller_id, amount, currency, quantity, last_updated
  from UNNEST(@SellerSales)
 on conflict (seller_id) do update
set   amount = seller_sales.amount + excluded.amount,
    quantity = seller_sales.quantity + excluded.quantity,
last_updated = excluded.last_updated;";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    salesSql,
                    new
                    {
                        SellerSales = sellerSales
                    },
                    transaction: transaction,
                    cancellationToken: token));

            await transaction.CommitAsync(token);
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
}
