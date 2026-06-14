namespace Contracts.Domain
{
    public enum OrderStatus
    {
        Draft,
        PendingPayment,
        PaymentAuthorised,
        Confirmed,
        Picking,
        Shipped,
        Delivered,
        CancelledByCustomer,
        CancelledPaymentFailed,
        CancelledNoStock
    }
}
