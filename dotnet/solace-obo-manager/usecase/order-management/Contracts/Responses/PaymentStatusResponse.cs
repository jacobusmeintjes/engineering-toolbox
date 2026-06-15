namespace Contracts.Responses
{
    public record PaymentStatusResponse(
    Guid OrderId,
    string Status,
    string? TransactionId,
    decimal Amount,
    string? FailureReason,
    DateTimeOffset CreatedAt);
}
