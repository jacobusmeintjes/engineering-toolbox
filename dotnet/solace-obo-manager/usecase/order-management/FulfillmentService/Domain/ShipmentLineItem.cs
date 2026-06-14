using Contracts.Domain;

namespace FulfillmentService.Domain
{
    // Domain/ShipmentLineItem.cs
    public class ShipmentLineItem
    {
        public Guid Id { get; private set; }
        public Guid ShipmentId { get; private set; }
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; } = default!;
        public string ProductName { get; private set; } = default!;
        public int Quantity { get; private set; }

        private ShipmentLineItem() { }  // EF

        public static ShipmentLineItem From(Guid shipmentId, OrderItem item) => new()
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            Quantity = item.Quantity
        };
    }
}
