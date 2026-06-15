namespace Messaging.Abstractions
{
    // Abstractions/IEvent.cs
    public interface IEvent
    {
        Guid EventId { get; }
        DateTimeOffset OccurredAt { get; }
        string EventType { get; }
    }
}
