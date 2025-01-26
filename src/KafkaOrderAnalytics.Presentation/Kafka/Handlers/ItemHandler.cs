using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Converters.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;

public class ItemHandler : IHandler<Ignore, OrderEvent>
{
    private readonly ILogger<ItemHandler> _logger;
    private readonly IItemRepository _itemRepository;
    private readonly IOrderEventConverter _orderEventConverter;

    public ItemHandler(
        ILogger<ItemHandler> logger, 
        IItemRepository itemRepository,
        IOrderEventConverter orderEventConverter)
    {
        _logger = logger;
        _itemRepository = itemRepository;
        _orderEventConverter = orderEventConverter;
    }

    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, OrderEvent>> messages, CancellationToken token)
    {
        foreach (var consumeResult in messages)
        {
            var inventoryItems = _orderEventConverter.ConvertToInventoryItems(consumeResult.Message.Value);
            switch (consumeResult.Message.Value.OrderStatus)
            {
                case OrderStatus.Delivered:
                    var sellerSales = _orderEventConverter.ConvertToSellerSales(consumeResult.Message.Value);
                    await _itemRepository.UpdateInventoryAndSales(
                        inventoryItems, sellerSales, token);
                    break;
                default:
                    await _itemRepository.UpdateInventory(inventoryItems, token);
                    break;
            }
            _logger.LogInformation(
                "Processed orderId: {OrderId} with status {Status}",
                consumeResult.Message.Value.OrderId, 
                consumeResult.Message.Value.OrderStatus);
        }
    }
}
