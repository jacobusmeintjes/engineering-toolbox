using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace MyApp.Modules.Orders.Internal.Models;

public sealed class Order
{
    private ConcurrentDictionary<Guid, OrderLineItem> _items = new();

    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount => Items.Sum(c => c.Quantity * c.UnitPrice);
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }

    public ReadOnlyCollection<OrderLineItem> Items => _items.Values.ToList().AsReadOnly();

    public static Order Create(Guid id, Guid customerId, OrderStatus status, DateTime createdAt) =>
        new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = status,
            CreatedAt = createdAt
        };


    public void AddLineItem(OrderLineItem item)
    {
        _items.TryAdd(item.ProductId, item);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}

public sealed record OrderLineItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice)
{
    public static OrderLineItem Create(Guid productId, int quantity, decimal unitPrice) =>
        new OrderLineItem(productId, quantity, unitPrice);
}