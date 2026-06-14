namespace Contracts.Domain
{
    // Domain/StockReservation.cs  
    public record StockReservation(
        string Sku,
        int Quantity);



    // Requests/CreateStockItemRequest.cs
    public record CreateStockItemRequest(
        string Sku,
        string ProductName,
        int Quantity);

    // Responses/StockLevelResponse.cs
    public record StockLevelResponse(
        string Sku,
        string ProductName,
        int TotalQuantity,
        int ReservedQuantity,
        int AvailableQuantity);
}
