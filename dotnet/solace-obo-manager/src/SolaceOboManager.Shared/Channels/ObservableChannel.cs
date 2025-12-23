using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace SolaceOboManager.Shared.Channels
{
    public sealed class ObservableChannel<T>
    {
        private readonly Channel<MessageEnvelope<T>> _channel;
        private readonly Meter _meter;
        private readonly Counter<long> _messagesPublished;
        private readonly Counter<long> _messagesProcessed;
        private readonly Counter<long> _messagesFailed;
        private readonly Histogram<double> _processingDuration;
        private readonly Histogram<double> _queueWaitTime;
        private readonly string _channelName;

        private long _queueDepth;

        public ObservableChannel(string channelName, int capacity = 1000)
        {
            _channelName = channelName;
            _channel = Channel.CreateBounded<MessageEnvelope<T>>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            _meter = new Meter($"Channel.{_channelName}", "1.0,0");

            // Counters
            _messagesPublished = _meter.CreateCounter<long>(
                "channel.messages.published",
                description: "Number of messages published to the channel");

            _messagesProcessed = _meter.CreateCounter<long>(
                "channel.messages.processed",
                description: "Number of messages successfully processed");

            _messagesFailed = _meter.CreateCounter<long>(
                "channel.messages.failed",
                description: "Number of messages that failed processing");

            // Histograms
            _processingDuration = _meter.CreateHistogram<double>(
                "channel.message.processing.duration",
                unit: "ms",
                description: "Time taken to process a message");

            _queueWaitTime = _meter.CreateHistogram<double>(
                "channel.message.queue.duration",
                unit: "ms",
                description: "Time message spent in queue before processing");

            // Observable Gauge for queue depth
            _meter.CreateObservableGauge(
                "channel.queue.depth",
                () => Interlocked.Read(ref _queueDepth),
                description: "Current number of messages in the queue");

        }

        public async ValueTask PublishAsync(T message, CancellationToken ct = default)
        {
            var envelope = new MessageEnvelope<T>
            {
                Message = message,
                EnqueuedAt = Stopwatch.GetTimestamp()
            };

            await _channel.Writer.WriteAsync(envelope, ct);

            Interlocked.Increment(ref _queueDepth);
            _messagesPublished.Add(1, new KeyValuePair<string, object?>("channel", _channelName));
        }

        public ChannelReader<MessageEnvelope<T>> Reader => _channel.Reader;

        public void RecordProcessed(MessageEnvelope<T> envelope, double processingMs)
        {
            Interlocked.Decrement(ref _queueDepth);

            var queueTimeMs = (Stopwatch.GetTimestamp() - envelope.EnqueuedAt)
                / (double)Stopwatch.Frequency * 1000;

            _messagesProcessed.Add(1, new KeyValuePair<string, object?>("channel", _channelName));
            _processingDuration.Record(processingMs, new KeyValuePair<string, object?>("channel", _channelName));
            _queueWaitTime.Record(queueTimeMs, new KeyValuePair<string, object?>("channel", _channelName));
        }

        public void RecordFailed(MessageEnvelope<T> envelope, Exception ex)
        {
            Interlocked.Decrement(ref _queueDepth);

            _messagesFailed.Add(1,
                new KeyValuePair<string, object?>("channel", _channelName),
                new KeyValuePair<string, object?>("error.type", ex.GetType().Name));
        }

        public void Complete() => _channel.Writer.Complete();
    }

    public class MessageEnvelope<T>
    {
        public required T Message { get; init; }
        public long EnqueuedAt { get; init; }
    }
}
