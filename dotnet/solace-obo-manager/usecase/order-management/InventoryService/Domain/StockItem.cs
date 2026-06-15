namespace InventoryService.Domain
{
    // Domain/StockItem.cs
    public class StockItem
    {
        public Guid Id { get; private set; }
        public string Sku { get; private set; } = default!;
        public string ProductName { get; private set; } = default!;
        public int TotalQuantity { get; private set; }
        public int ReservedQuantity { get; private set; }
        public int AvailableQuantity => TotalQuantity - ReservedQuantity;

        private StockItem() { }  // EF

        public static StockItem Create(string sku, string productName, int quantity) => new()
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            ProductName = productName,
            TotalQuantity = quantity,
            ReservedQuantity = 0
        };

        public void Reserve(int quantity)
        {
            if (quantity > AvailableQuantity)
                throw new InvalidOperationException(
                    $"Cannot reserve {quantity} units of {Sku} — only {AvailableQuantity} available");

            ReservedQuantity += quantity;
        }

        public void Release(int quantity)
        {
            if (quantity > ReservedQuantity)
                throw new InvalidOperationException(
                    $"Cannot release {quantity} units of {Sku} — only {ReservedQuantity} reserved");

            ReservedQuantity -= quantity;
        }

        public void Adjust(int quantity)
        {
            if (TotalQuantity + quantity < ReservedQuantity)
                throw new InvalidOperationException(
                    $"Cannot adjust {Sku} below reserved quantity of {ReservedQuantity}");

            TotalQuantity += quantity;
        }
    }
}
