namespace Contracts.Responses
{
    // Responses/ReserveStockResponse.cs
    public record ReserveStockResponse(
        bool Success,
        IReadOnlyList<string> OutOfStockSkus);

}
