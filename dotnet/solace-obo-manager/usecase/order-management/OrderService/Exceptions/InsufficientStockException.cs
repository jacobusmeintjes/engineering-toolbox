namespace OrderService.Exceptions
{
    // Exceptions/InsufficientStockException.cs
    public class InsufficientStockException(IReadOnlyList<string> outOfStockSkus)
        : Exception($"Insufficient stock for SKUs: {string.Join(", ", outOfStockSkus)}")
    {
        public IReadOnlyList<string> OutOfStockSkus { get; } = outOfStockSkus;
    }
}
