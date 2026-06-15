namespace PaymentService.Services
{

    // Services/GatewayResult.cs
    public record GatewayResult(
        bool Success,
        string? TransactionId,
        string? FailureReason);

    // Services/StripePaymentGateway.cs  — stub, replace with real Stripe SDK calls
    public class StripePaymentGateway(ILogger<StripePaymentGateway> logger) : IPaymentGateway
    {
        public Task<GatewayResult> AuthoriseAsync(
            string paymentMethodToken, decimal amount, CancellationToken ct)
        {
            // Replace with: StripeClient.PaymentIntents.CreateAsync(...)
            logger.LogInformation("Authorising payment of {Amount} for token {Token}",
                amount, paymentMethodToken);

            return Task.FromResult(new GatewayResult(
                Success: true,
                TransactionId: $"txn_{Guid.NewGuid():N}",
                FailureReason: null));
        }

        public Task VoidAsync(string transactionId, CancellationToken ct)
        {
            // Replace with: StripeClient.PaymentIntents.CancelAsync(transactionId)
            logger.LogInformation("Voiding transaction {TransactionId}", transactionId);
            return Task.CompletedTask;
        }
    }
}
