namespace Contracts.Responses
{
    // Responses/AuthorisePaymentResponse.cs
    public record AuthorisePaymentResponse(
        bool Success,
        string? TransactionId,
        string? FailureReason);
}
