using Contracts.Domain;

namespace Contracts.Requests
{
    // Oms.Contracts/Requests/PlaceOrderRequest.cs
    public record PlaceOrderRequest(
        Guid CustomerId,
        string ShippingAddress,
        IReadOnlyList<OrderItem> Items,
        string PaymentMethodToken);   // tokenised — never raw card data

}
