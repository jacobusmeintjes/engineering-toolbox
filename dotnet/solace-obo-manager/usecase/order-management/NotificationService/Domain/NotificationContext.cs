namespace NotificationService.Domain
{
    // Domain/NotificationContext.cs
    public record NotificationContext(
        Guid CustomerId,
        Guid OrderId,
        string? CustomerName,
        decimal? OrderTotal);
}
