using System.Diagnostics;

namespace SolaceOboManager.Shared.Channels
{
    public class ObservableChannelWithTracing<T>
    {
        private readonly ObservableChannel<T> _channel;
        private static readonly ActivitySource ActivitySource = new("Channel.Processing");

        public ObservableChannelWithTracing(string channelName, int capacity = 1000)
        {
            _channel = new ObservableChannel<T>(channelName, capacity);
        }

        public async ValueTask PublishAsync(T message, CancellationToken ct = default)
        {
            using var activity = ActivitySource.StartActivity("channel.publish", ActivityKind.Producer);
            activity?.SetTag("channel.name", _channel.GetType().Name);
            activity?.SetTag("message.type", typeof(T).Name);

            try
            {
                await _channel.PublishAsync(message, ct);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                throw;
            }
        }

        public async Task ProcessMessagesAsync(
            Func<T, CancellationToken, Task> handler,
            CancellationToken ct = default)
        {
            await foreach (var envelope in _channel.Reader.ReadAllAsync(ct))
            {
                using var activity = ActivitySource.StartActivity("channel.process", ActivityKind.Consumer);
                activity?.SetTag("channel.name", _channel.GetType().Name);
                activity?.SetTag("message.type", typeof(T).Name);

                var sw = Stopwatch.StartNew();

                try
                {
                    await handler(envelope.Message, ct);

                    sw.Stop();
                    _channel.RecordProcessed(envelope, sw.Elapsed.TotalMilliseconds);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _channel.RecordFailed(envelope, ex);

                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);

                    // Don't rethrow - let it continue processing
                }
            }
        }
    }
}
