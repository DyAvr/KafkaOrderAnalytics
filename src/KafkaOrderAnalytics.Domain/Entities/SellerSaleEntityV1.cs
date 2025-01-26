using System;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Entities;

public class SellerSaleEntityV1
{
    public required long SellerId { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required int Quantity { get; set; }
    public required DateTimeOffset LastUpdated { get; set; }
}