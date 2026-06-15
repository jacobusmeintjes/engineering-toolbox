using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Abstractions
{

    // Abstractions/EventBase.cs
    public abstract record EventBase : IEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
        public abstract string EventType { get; }
    }
}
