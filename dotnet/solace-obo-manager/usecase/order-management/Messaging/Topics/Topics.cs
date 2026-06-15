using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Topics
{
    // Topics/Topics.cs
    public static class Topics
    {
        private const string Root = "oms";

        public static class Orders
        {
            private const string Base = $"{Root}/orders/v1";
            public const string Placed = $"{Base}/placed";
            public const string Confirmed = $"{Base}/confirmed";
            public const string Cancelled = $"{Base}/cancelled";

            public const string All = $"{Root}/orders/>";
        }

        public static class Payments
        {
            private const string Base = $"{Root}/payments/v1";
            public const string Authorised = $"{Base}/authorised";
            public const string Failed = $"{Base}/failed";
            public const string Voided = $"{Base}/voided";

            public const string All = $"{Root}/payments/>";
        }

        public static class Inventory
        {
            private const string Base = $"{Root}/inventory/v1";
            public const string Reserved = $"{Base}/reserved";
            public const string ReservationFailed = $"{Base}/reservation-failed";
            public const string Released = $"{Base}/released";

            public const string All = $"{Root}/inventory/>";
        }

        public static class Fulfilment
        {
            private const string Base = $"{Root}/fulfilment/v1";
            public const string ShipmentCreated = $"{Base}/shipment-created";
            public const string ShipmentPacked = $"{Base}/shipment-packed";
            public const string ShipmentShipped = $"{Base}/shipment-shipped";
            public const string ShipmentDelivered = $"{Base}/shipment-delivered";

            public const string All = $"{Root}/fulfilment/>";
        }
    }
}
