using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SolaceOboManager.Benchmarks.Channels
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class ChannelBenchmarks
    {
        private const int MessageCount = 100_000;

        [Benchmark(Baseline = true)]
        public async Task UnboundedChannel_SingleProducerSingleConsumer()
        {
            var channel = Channel.CreateUnbounded<int>();

            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < MessageCount; i++)
                {
                    await channel.Writer.WriteAsync(i);
                }
                channel.Writer.Complete();
            });

            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    // Simulate minimal work
                    _ = item;
                }
            });

            await Task.WhenAll(producer, consumer);
        }

        [Benchmark]
        public async Task BoundedChannel_SingleProducerSingleConsumer()
        {
            var channel = Channel.CreateBounded<int>(1000);

            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < MessageCount; i++)
                {
                    await channel.Writer.WriteAsync(i);
                }
                channel.Writer.Complete();
            });

            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    _ = item;
                }
            });

            await Task.WhenAll(producer, consumer);
        }

        [Benchmark]
        public async Task UnboundedChannel_MultipleConsumers()
        {
            var channel = Channel.CreateUnbounded<int>();
            int consumerCount = 4;

            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < MessageCount; i++)
                {
                    await channel.Writer.WriteAsync(i);
                }
                channel.Writer.Complete();
            });

            var consumers = Enumerable.Range(0, consumerCount)
                .Select(_ => Task.Run(async () =>
                {
                    await foreach (var item in channel.Reader.ReadAllAsync())
                    {
                        _ = item;
                    }
                }))
                .ToArray();

            await Task.WhenAll(producer);
            await Task.WhenAll(consumers);
        }
    }

}
