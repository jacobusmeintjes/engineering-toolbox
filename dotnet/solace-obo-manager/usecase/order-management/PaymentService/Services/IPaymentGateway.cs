namespace PaymentService.Services
{
    // Services/IPaymentGateway.cs
    public interface IPaymentGateway
    {
        Task<GatewayResult> AuthoriseAsync(
            string paymentMethodToken, decimal amount, CancellationToken ct);

        Task VoidAsync(string transactionId, CancellationToken ct);
    }
}
