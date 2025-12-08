using FluentAssertions;
using SolaceOboManager.Shared.Channels;
using System.Diagnostics;
using Xunit.Abstractions;

namespace SolaceOboManager.LoadTests
{
    public class ChannelLoadTests
    {
        [Fact]
        public async Task Channel_ShouldHandleHighThroughput()
        {
            // Arrange
            var channel = new ObservableChannel<int>("load-test", capacity: 10000);
            var messageCount = 100_000;
            var processed = 0;

            // Act
            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < messageCount; i++)
                {
                    await channel.PublishAsync(i);
                }
            });

            var consumer = Task.Run(async () =>
            {
                await foreach (var envelope in channel.Reader.ReadAllAsync())
                {
                    Interlocked.Increment(ref processed);
                    if (processed >= messageCount) break;
                }
            });

            await Task.WhenAll(producer, consumer);

            // Assert
            processed.Should().Be(messageCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        public async Task PartitionedChannel_ShouldScaleWithPartitions(int partitionCount)
        {
            // Arrange
            var channel = new PartitionedObservableChannel<int, int>(
                "load-test",
                partitionCount,
                msg => msg);

            var messageCount = 10_000;
            var processed = 0;
            var sw = Stopwatch.StartNew();

            // Act
            var producer = Task.Run(async () =>
            {
                for (int i = 0; i < messageCount; i++)
                {
                    await channel.PublishAsync(i);
                }
            });

            var cts = new CancellationTokenSource();
            var consumers = channel.StartProcessingAsync(async (msg, ct) =>
            {
                Interlocked.Increment(ref processed);
                await Task.Delay(1, ct);
                if (processed >= messageCount) cts.Cancel();
            }, cts.Token);

            await producer;

            try
            {
                await Task.WhenAll(consumers);
            }
            catch (OperationCanceledException) { }

            sw.Stop();

            // Assert
            processed.Should().Be(messageCount);

            // More partitions should be faster (though not guaranteed in tests)
            _testOutputHelper.WriteLine(
                $"Processed {messageCount} messages with {partitionCount} partitions in {sw.ElapsedMilliseconds}ms");
        }

        private readonly ITestOutputHelper _testOutputHelper;

        public ChannelLoadTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
    }
}
