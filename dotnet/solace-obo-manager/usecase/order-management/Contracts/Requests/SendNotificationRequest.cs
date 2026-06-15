namespace Contracts.Requests
{
    // Requests/SendNotificationRequest.cs
    public record SendNotificationRequest(
        Guid CustomerId,
        string EventType,           // "OrderConfirmed", "OrderShipped", etc.
        Guid OrderId,
        decimal? OrderTotal);
}
