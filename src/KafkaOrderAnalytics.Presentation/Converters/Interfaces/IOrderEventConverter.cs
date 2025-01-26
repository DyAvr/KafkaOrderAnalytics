using System.Collections.Generic;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters.Interfaces;

public interface IOrderEventConverter
{
    IEnumerable<InventoryItemEntityV1> ConvertToInventoryItems(OrderEvent orderEvent);
    IEnumerable<SellerSaleEntityV1> ConvertToSellerSales(OrderEvent orderEvent);
}