using FluentAssertions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using SolaceOboManager.Shared.Channels;
using System.Diagnostics;

namespace SolaceOboManager.Tests
{
    public class ObservableChannelMetricsTests
    {
        [Fact]
        public async Task RecordProcessed_ShouldIncrementProcessedCounter()
        {
            // Arrange
            var metrics = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Channel.*")
                .AddInMemoryExporter(metrics)
                .Build();

            var channel = new ObservableChannel<string>("test-channel");
            var envelope = new MessageEnvelope<string>
            {
                Message = "test",
                EnqueuedAt = Stopwatch.GetTimestamp()
            };

            // Act
            channel.RecordProcessed(envelope, 100);
            meterProvider.ForceFlush();

            // Assert
            var processedMetric = metrics
                .FirstOrDefault(m => m.Name == "channel.messages.processed");

            processedMetric.Should().NotBeNull();
            GetCounterValue(processedMetric!).Should().Be(1);
        }

        [Fact]
        public async Task RecordFailed_ShouldIncrementFailedCounter()
        {
            // Arrange
            var metrics = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Channel.*")
                .AddInMemoryExporter(metrics)
                .Build();

            var channel = new ObservableChannel<string>("test-channel");
            var envelope = new MessageEnvelope<string>
            {
                Message = "test",
                EnqueuedAt = Stopwatch.GetTimestamp()
            };

            // Act
            channel.RecordFailed(envelope, new InvalidOperationException("Test error"));
            meterProvider.ForceFlush();

            // Assert
            var failedMetric = metrics
                .FirstOrDefault(m => m.Name == "channel.messages.failed");

            failedMetric.Should().NotBeNull();
            GetCounterValue(failedMetric!).Should().Be(1);
        }

        [Fact]
        public async Task QueueDepth_ShouldReflectCurrentState()
        {
            // Arrange
            var metrics = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("Channel.*")
                .AddInMemoryExporter(metrics)
                .Build();

            var channel = new ObservableChannel<string>("test-channel");

            // Act - Add messages
            await channel.PublishAsync("msg1");
            await channel.PublishAsync("msg2");
            await channel.PublishAsync("msg3");

            meterProvider.ForceFlush();

            // Assert
            var queueDepthMetric = metrics
                .FirstOrDefault(m => m.Name == "channel.queue.depth");

            queueDepthMetric.Should().NotBeNull();
            GetGaugeValue(queueDepthMetric!).Should().Be(3);

            // Process one message
            var envelope = await channel.Reader.ReadAsync();
            channel.RecordProcessed(envelope, 10);

            metrics.Clear();
            meterProvider.ForceFlush();

            queueDepthMetric = metrics
                .FirstOrDefault(m => m.Name == "channel.queue.depth");
            GetGaugeValue(queueDepthMetric!).Should().Be(2);
        }

        private static long GetCounterValue(Metric metric)
        {
            var sum = 0L;
            foreach (var metricPoint in metric.GetMetricPoints())
            {
                sum += metricPoint.GetSumLong();
            }
            return sum;
        }

        private static long GetGaugeValue(Metric metric)
        {
            foreach (var metricPoint in metric.GetMetricPoints())
            {
                return metricPoint.GetGaugeLastValueLong();
            }
            return 0;
        }
    }
}
