namespace InventoryService.Domain
{
    // Domain/StockReservationRecord.cs
    public class StockReservationRecord
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string Sku { get; private set; } = default!;
        public int Quantity { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ReleasedAt { get; private set; }

        private StockReservationRecord() { }  // EF

        public static StockReservationRecord Create(
            Guid orderId, string sku, int quantity) => new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Sku = sku,
                Quantity = quantity,
                Status = ReservationStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

        public void Release()
        {
            if (Status != ReservationStatus.Active)
                throw new InvalidOperationException(
                    $"Cannot release a reservation in status {Status}");

            Status = ReservationStatus.Released;
            ReleasedAt = DateTimeOffset.UtcNow;
        }
    }
}
