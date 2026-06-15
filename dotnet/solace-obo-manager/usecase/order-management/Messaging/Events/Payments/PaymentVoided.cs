using Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Events.Payments
{

    // Events/Payments/PaymentVoided.cs
    public record PaymentVoided : EventBase
    {
        public override string EventType => "oms.payments.voided";

        public required Guid OrderId { get; init; }
        public required string TransactionId { get; init; }
    }
}
