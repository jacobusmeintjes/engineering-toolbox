namespace MyApp.Modules.Orders.Public.Dtos;

/// <summary>
/// Public representation of an Order for cross-module communication.
/// This is a DTO - it has no behavior, only data.
/// </summary>
public sealed class OrderDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required OrderStatusDto Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<OrderLineItemDto> Items { get; init; }
}

public sealed class OrderLineItemDto
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}

/// <summary>
/// Public enum - separate from internal OrderStatus to avoid coupling.
/// </summary>
public enum OrderStatusDto
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}