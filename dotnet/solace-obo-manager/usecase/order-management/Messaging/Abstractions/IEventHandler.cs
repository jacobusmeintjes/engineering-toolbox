namespace Messaging.Abstractions
{
    // Abstractions/IEventHandler.cs
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken ct);
    }
}
