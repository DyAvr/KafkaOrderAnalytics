using System.Collections.Generic;
using System.Linq;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters;

public class OrderEventConverter : IOrderEventConverter
{
    private const string russianCurrency = "RUB";
    private const string kazakhstanCurrency = "KZT";
    private const decimal unitsMultiplier = 1_000_000_000.0m;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderEventConverter(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }
    
    public IEnumerable<InventoryItemEntityV1> ConvertToInventoryItems(OrderEvent orderEvent)
    {
        return orderEvent.Positions
            .Select(p => new InventoryItemEntityV1
            {
                ItemId = p.ItemId,
                Reserved = p.Quantity * (orderEvent.OrderStatus == OrderStatus.Created ? 1 : -1),
                Sold = orderEvent.OrderStatus == OrderStatus.Delivered ? p.Quantity : 0,
                Cancelled = orderEvent.OrderStatus == OrderStatus.Cancelled ? p.Quantity : 0,
                LastUpdated = _dateTimeProvider.Now()
            })
            .GroupBy(p => p.ItemId)
            .Select(g => new InventoryItemEntityV1
            {
                ItemId = g.Key,
                Reserved = g.Sum(item => item.Reserved),
                Sold = g.Sum(item => item.Sold),
                Cancelled = g.Sum(item => item.Cancelled),
                LastUpdated = g.Max(item => item.LastUpdated)
            })
            .ToArray();
    }

    public IEnumerable<SellerSaleEntityV1> ConvertToSellerSales(OrderEvent orderEvent)
    {
        return orderEvent.Positions
            .Where(p => orderEvent.OrderStatus == OrderStatus.Delivered)
            .Select(p => new SellerSaleEntityV1
            {
                SellerId = long.Parse(p.ItemId.ToString().Substring(0, 6)),
                Amount = (p.Price.Units + p.Price.Nanos / unitsMultiplier) * p.Quantity,
                Currency = p.Price.Currency,
                Quantity = p.Quantity,
                LastUpdated = _dateTimeProvider.Now()
            })
            .GroupBy(p => new {p.SellerId, p.Currency})
            .Select(g => new SellerSaleEntityV1
            {
                SellerId = g.Key.SellerId,
                Amount = g.Sum(item => item.Amount),
                Currency = g.Key.Currency,
                Quantity = g.Sum(item => item.Quantity),
                LastUpdated = g.Max(item => item.LastUpdated)
            })
            .ToArray();
    }
}