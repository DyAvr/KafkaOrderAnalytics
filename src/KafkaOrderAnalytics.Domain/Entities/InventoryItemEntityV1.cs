using System;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public record InventoryItemEntityV1
{
    public required long ItemId { get; init; }
    public required int Reserved { get; init; }
    public required int Sold { get; init; }
    public required int Cancelled { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
}