namespace Contracts.Domain
{
    // Oms.Contracts/Models/OrderItem.cs
    public record OrderItem(
        Guid ProductId,
        string Sku,
        string ProductName,
        int Quantity,
        decimal UnitPrice);

}
