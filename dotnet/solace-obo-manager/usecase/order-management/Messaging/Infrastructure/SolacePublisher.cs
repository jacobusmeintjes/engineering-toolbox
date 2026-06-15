using Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Solace.Messaging.Trace.Propagation;
using SolaceSystems.Solclient.Messaging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Messaging.Infrastructure
{
    // Infrastructure/SolacePublisher.cs
    public class SolacePublisher : IEventPublisher
    {
        private readonly SolaceConnection _connection;
        private readonly ILogger<SolacePublisher> _logger;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly CompositeTextMapPropagator Propagator =
       new(new List<TextMapPropagator>
       {
            new TraceContextPropagator(),
            new BaggagePropagator()
       });

        public SolacePublisher(
            SolaceConnection connection,
            ILogger<SolacePublisher> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public Task PublishAsync<TEvent>(
            TEvent @event,
            string topic,
            CancellationToken ct) where TEvent : IEvent
        {
            using var activity = MessagingTelemetry.Source.StartActivity($"publish {topic}", ActivityKind.Producer);


            activity?.SetTag("messaging.system", "solace");
            activity?.SetTag("messaging.destination", topic);
            activity?.SetTag("messaging.destination_kind", "topic");
            activity?.SetTag("messaging.operation", "publish");
            activity?.SetTag("oms.event.type", @event.EventType);
            activity?.SetTag("oms.event.id", @event.EventId.ToString());


            // Belt-and-braces guard — InitialiseAsync should have run via hosted service
            var session = _connection.Session
                ?? throw new InvalidOperationException(
                    "Cannot publish — Solace session not initialised. " +
                    "Ensure AddOmsMessaging is registered and the host has started.");


            var json = JsonSerializer.Serialize(@event, SerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            using var message = ContextFactory.Instance.CreateMessage();
            message.Destination = ContextFactory.Instance.CreateTopic(topic);
            message.BinaryAttachment = bytes;
            message.DeliveryMode = MessageDeliveryMode.Persistent;

            // Correlate published messages in logs
            message.ApplicationMessageId = @event.EventId.ToString();
            message.ApplicationMessageType = @event.EventType;

            // Inject W3C trace context and baggage using the official Solace carrier
            Propagator.Inject(
                new PropagationContext(
                    activity?.Context ?? Activity.Current?.Context ?? default,
                    Baggage.Current),
                message,
                SolaceMessageCarrier.Setter);  // ← official Solace setter

            var result = _connection.Session.Send(message);

            if (result != ReturnCode.SOLCLIENT_OK)
            {
                activity?.SetStatus(ActivityStatusCode.Error,
                $"Solace publish failed: {result}");


                _logger.LogError(
                    "Failed to publish {EventType} to topic {Topic} — result: {Result}",
                    @event.EventType, topic, result);

                throw new InvalidOperationException(
                    $"Solace publish failed for topic {topic}: {result}");
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.Stop();


            _logger.LogInformation(
                "Published {EventType} event {EventId} to topic {Topic}",
                @event.EventType, @event.EventId, topic);

            return Task.CompletedTask;
        }
    }
}
