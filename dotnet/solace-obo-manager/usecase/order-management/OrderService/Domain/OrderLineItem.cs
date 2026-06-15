using Contracts.Domain;

namespace OrderService.Domain
{
    // Domain/OrderLineItem.cs  — lives in OrderService, not Contracts
    public class OrderLineItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; } = default!;
        public string ProductName { get; private set; } = default!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        private OrderLineItem() { }  // EF

        public static OrderLineItem From(Guid orderId, OrderItem item) => new()
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = item.ProductId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };

        public OrderItem ToContractItem() => new(ProductId, Sku, ProductName, Quantity, UnitPrice);
    }
}
