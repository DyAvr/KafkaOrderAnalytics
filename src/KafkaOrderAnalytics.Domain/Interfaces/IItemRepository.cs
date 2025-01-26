using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

public interface IItemRepository
{
    Task UpdateInventory(IEnumerable<InventoryItemEntityV1> inventoryItems, CancellationToken token);
    Task UpdateInventoryAndSales(IEnumerable<InventoryItemEntityV1> inventoryItems, IEnumerable<SellerSaleEntityV1> sellerSales, CancellationToken token);
}
