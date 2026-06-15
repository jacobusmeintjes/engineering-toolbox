using Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using Solace.Messaging.Trace.Propagation;
using SolaceSystems.Solclient.Messaging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Messaging.Infrastructure
{
    // Infrastructure/SolaceSubscriber.cs
    public abstract class SolaceSubscriber<TEvent> : BackgroundService
        where TEvent : class, IEvent
    {
        private readonly SolaceConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly string _topic;
        private IQueue? _queue;
        private IFlow? _flow;

        private static readonly CompositeTextMapPropagator Propagator =
        new(new List<TextMapPropagator>
        {
            new TraceContextPropagator(),
            new BaggagePropagator()
        });


        private static readonly JsonSerializerOptions DeserializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected SolaceSubscriber(
            SolaceConnection connection,
            IServiceScopeFactory scopeFactory,
            ILogger logger,
            string topic)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _topic = topic;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await _connection.InitialiseAsync(ct);

            // Queue name derived from topic + service suffix
            // e.g. "oms.inventory.v1.reservation-failed.payment-service"
            var sanitisedTopic = _topic.Replace("/", ".");
            var queueName = $"{sanitisedTopic}.{QueueSuffix}";

            _queue = ContextFactory.Instance.CreateQueue(queueName);

            // Provision the queue on the broker if it doesn't exist
            var endpointProps = new EndpointProperties
            {
                Permission = EndpointProperties.EndpointPermission.Consume,
                AccessType = EndpointProperties.EndpointAccessType.Exclusive
            };

            _connection.Session.Provision(
                _queue,
                endpointProps,
                ProvisionFlag.IgnoreErrorIfEndpointAlreadyExists,
                null);

            // Bind the topic to the queue
            var topicSubscription = ContextFactory.Instance.CreateTopic(_topic);
            _connection.Session.Subscribe(
                _queue,
                topicSubscription,
                SubscribeFlag.WaitForConfirm,
                null);

            _logger.LogInformation(
                "Subscribed to topic {Topic} via queue {Queue}",
                _topic, queueName);

            // Start a flow to consume messages from the queue
            var flowProps = new FlowProperties
            {
                AckMode = MessageAckMode.ClientAck  // we ack only after successful handling
            };

            _flow = _connection.Session.CreateFlow(
                flowProps,
                _queue,
                null,
                OnMessage,
                OnFlowEvent);

            _flow.Start();

            // Keep alive until cancellation
            await Task.Delay(Timeout.Infinite, ct).ConfigureAwait(false);

            _flow.Stop();
            _flow.Dispose();
        }

        private async void OnMessage(object? sender, MessageEventArgs e)
        {
            using var message = e.Message;

            // Extract W3C trace context and baggage using the official Solace getter
            var parentContext = Propagator.Extract(
                default,
                message,
                SolaceMessageCarrier.Getter);  // ← official Solace getter

            Baggage.Current = parentContext.Baggage;

            // Start consumer span as a child of the publisher span
            using var activity = MessagingTelemetry.Source.StartActivity(
                $"consume {_topic}",
                ActivityKind.Consumer,
                parentContext.ActivityContext);

            activity?.SetTag("messaging.system", "solace");
            activity?.SetTag("messaging.destination", _topic);
            activity?.SetTag("messaging.destination_kind", "topic");
            activity?.SetTag("messaging.operation", "consume");
            activity?.SetTag("messaging.message_id",
                message.ApplicationMessageId);
            activity?.SetTag("oms.event.type",
                message.ApplicationMessageType);



            try
            {
                var json = Encoding.UTF8.GetString(message.BinaryAttachment);
                var @event = JsonSerializer.Deserialize<TEvent>(json, DeserializerOptions);

                if (@event is null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, $"Received null after deserialising {typeof(TEvent).Name} message");

                    _logger.LogWarning(
                        "Received null after deserialising {EventType} message", typeof(TEvent).Name);
                    _flow?.Ack(message.ADMessageId);
                    return;
                }

                activity?.SetTag("oms.event.id", @event.EventId.ToString());

                _logger.LogInformation(
                    "Received {EventType} event {EventId}",
                    @event.EventType, @event.EventId);

                // Create a DI scope per message — handlers can use scoped services (DbContext etc.)
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<IEventHandler<TEvent>>();

                await handler.HandleAsync(@event, CancellationToken.None);

                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.Stop();

                // Ack only after successful handling
                _flow?.Ack(message.ADMessageId);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);


                _logger.LogError(ex,
                    "Failed to process {EventType} message — message will not be acked",
                    typeof(TEvent).Name);

                // Do not ack — Solace will redeliver based on queue configuration
            }
        }

        private void OnFlowEvent(object? sender, FlowEventArgs e)
        {
            switch (e.Event)
            {
                case FlowEvent.UpNotice:
                    _logger.LogInformation("Solace flow up for {EventType}", typeof(TEvent).Name);
                    break;

                case FlowEvent.DownError:
                    _logger.LogError(
                        "Solace flow down for {EventType} — info: {Info}",
                        typeof(TEvent).Name, e.Info);
                    break;

                case FlowEvent.Reconnecting:
                    _logger.LogWarning(
                        "Solace flow reconnecting for {EventType}", typeof(TEvent).Name);
                    break;
            }
        }

        // Each subscriber subclass provides a unique suffix so queues don't collide
        protected abstract string QueueSuffix { get; }
    }
}
