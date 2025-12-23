using SolaceOboManager.Shared.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace SolaceOboManager.IntegrationTests
{
    public static class ChannelTestHelpers
    {
        public static async Task<List<T>> ReadAllAsync<T>(
            this ChannelReader<MessageEnvelope<T>> reader,
            int maxItems = 100,
            TimeSpan? timeout = null)
        {
            var results = new List<T>();
            var timeoutToken = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5)).Token;

            try
            {
                await foreach (var envelope in reader.ReadAllAsync(timeoutToken))
                {
                    results.Add(envelope.Message);
                    if (results.Count >= maxItems) break;
                }
            }
            catch (OperationCanceledException)
            {
                // Timeout reached
            }

            return results;
        }

        public static async Task PublishManyAsync<T>(
            this ObservableChannel<T> channel,
            IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                await channel.PublishAsync(message);
            }
        }
    }
}
