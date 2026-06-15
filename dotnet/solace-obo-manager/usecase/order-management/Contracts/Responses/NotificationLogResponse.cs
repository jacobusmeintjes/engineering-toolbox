namespace Contracts.Responses
{
    // Oms.Contracts/Responses/NotificationLogResponse.cs
    public record NotificationLogResponse(
        Guid NotificationId,
        Guid CustomerId,
        Guid OrderId,
        string EventType,
        string Channel,
        string Status,
        string? FailureReason,
        DateTimeOffset SentAt);

}
