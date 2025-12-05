using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SolaceOboManager.Benchmarks.Channels
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class PartitionedChannelBenchmarks
    {
        private const int MessageCount = 100_000;

        [Params(1, 4, 8, 16)]
        public int PartitionCount { get; set; }

        [Benchmark]
        public async Task PartitionedChannels()
        {
            var partitions = Enumerable.Range(0, PartitionCount)
                .Select(_ => Channel.CreateUnbounded<int>())
                .ToArray();

            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < MessageCount; i++)
                {
                    var partition = i % PartitionCount;
                    await partitions[partition].Writer.WriteAsync(i);
                }

                foreach (var channel in partitions)
                {
                    channel.Writer.Complete();
                }
            });

            var consumers = partitions.Select(channel =>
                Task.Run(async () =>
                {
                    await foreach (var item in channel.Reader.ReadAllAsync())
                    {
                        _ = item;
                    }
                }))
                .ToArray();

            await producer;
            await Task.WhenAll(consumers);
        }
    }
}
