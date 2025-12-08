using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;

namespace SolaceOboManager.Shared.Channels
{
    public class PartitionedObservableChannel<TKey, TMessage> where TKey : notnull
    {
        private readonly ObservableChannel<TMessage>[] _partitions;
        private readonly Func<TMessage, TKey> _keySelector;
        private readonly Meter _meter;
        private readonly Counter<long> _partitionHits;

        public PartitionedObservableChannel(
            string channelName,
            int partitionCount,
            Func<TMessage, TKey> keySelector)
        {
            _keySelector = keySelector;
            _meter = new Meter($"PartitionedChannel.{channelName}");

            _partitions = Enumerable.Range(0, partitionCount)
                .Select(i => new ObservableChannel<TMessage>($"{channelName}.partition.{i}"))
                .ToArray();

            _partitionHits = _meter.CreateCounter<long>(
                "channel.partition.hits",
                description: "Number of messages routed to each partition");
        }

        public async ValueTask PublishAsync(TMessage message, CancellationToken ct = default)
        {
            var key = _keySelector(message);
            var partitionIndex = Math.Abs(key.GetHashCode()) % _partitions.Length;

            _partitionHits.Add(1,
                new KeyValuePair<string, object?>("partition", partitionIndex));

            await _partitions[partitionIndex].PublishAsync(message, ct);
        }

        public Task[] StartProcessingAsync(
            Func<TMessage, CancellationToken, Task> handler,
            CancellationToken ct)
        {
            return _partitions.Select((partition, index) =>
                Task.Run(async () =>
                {
                    await foreach (var envelope in partition.Reader.ReadAllAsync(ct))
                    {
                        using var activity = ActivitySource.StartActivity($"partition.{index}.process");
                        activity?.SetTag("partition.index", index);

                        var sw = Stopwatch.StartNew();
                        try
                        {
                            await handler(envelope.Message, ct);
                            sw.Stop();
                            partition.RecordProcessed(envelope, sw.Elapsed.TotalMilliseconds);
                        }
                        catch (Exception ex)
                        {
                            sw.Stop();
                            partition.RecordFailed(envelope, ex);
                        }
                    }
                }, ct))
                .ToArray();
        }

        private static readonly ActivitySource ActivitySource = new("PartitionedChannel.Processing");
    }
}
