namespace MyApp.Modules.Orders.Public.Dtos;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    List<OrderLineItemDto> Items)
{
    public CreateOrderRequest() : this(Guid.Empty, new List<OrderLineItemDto>())
    {
        
    }
}
