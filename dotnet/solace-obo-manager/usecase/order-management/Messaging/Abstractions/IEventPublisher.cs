namespace Messaging.Abstractions
{
    // Abstractions/IEventPublisher.cs
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, string topic, CancellationToken ct)
            where TEvent : IEvent;
    }
}
