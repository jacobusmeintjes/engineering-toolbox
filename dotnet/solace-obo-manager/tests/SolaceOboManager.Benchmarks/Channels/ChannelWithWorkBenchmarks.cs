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
    public class ChannelWithWorkBenchmarks
    {
        private const int MessageCount = 10_000;

        [Benchmark]
        public async Task Channel_WithCpuBoundWork()
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

            var consumers = Enumerable.Range(0, 4)
                .Select(_ => Task.Run(async () =>
                {
                    await foreach (var item in channel.Reader.ReadAllAsync())
                    {
                        // Simulate CPU work
                        var hash = 0;
                        for (int i = 0; i < 100; i++)
                        {
                            hash ^= item.GetHashCode();
                        }
                    }
                }))
                .ToArray();

            await producer;
            await Task.WhenAll(consumers);
        }

        [Benchmark]
        public async Task Channel_WithIoBoundWork()
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

            var consumers = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(async () =>
                {
                    await foreach (var item in channel.Reader.ReadAllAsync())
                    {
                        // Simulate I/O work
                        await Task.Delay(1);
                    }
                }))
                .ToArray();

            await producer;
            await Task.WhenAll(consumers);
        }
    }
}
