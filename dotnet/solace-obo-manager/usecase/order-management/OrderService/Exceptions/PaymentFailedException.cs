namespace OrderService.Exceptions
{
    // Exceptions/PaymentFailedException.cs
    public class PaymentFailedException(string? reason)
        : Exception($"Payment authorisation failed: {reason ?? "unknown reason"}")
    {
        public string? Reason { get; } = reason;
    }
}
