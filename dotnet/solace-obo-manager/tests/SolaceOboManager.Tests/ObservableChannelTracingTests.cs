using FluentAssertions;
using OpenTelemetry;
using OpenTelemetry.Trace;
using SolaceOboManager.Shared.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SolaceOboManager.Tests
{
    public class ObservableChannelTracingTests
    {
        [Fact]
        public async Task PublishAsync_ShouldCreateProducerActivity()
        {
            // Arrange
            var activities = new List<Activity>();
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("Channel.Processing")
                .AddInMemoryExporter(activities)
                .Build();

            var channel = new ObservableChannelWithTracing<string>("test-channel");

            // Act
            await channel.PublishAsync("test message");
            tracerProvider.ForceFlush();

            // Assert
            activities.Should().ContainSingle(a => a.OperationName == "channel.publish");
            var activity = activities.First();
            activity.Kind.Should().Be(ActivityKind.Producer);
            activity.Status.Should().Be(ActivityStatusCode.Ok);
            activity.Tags.Should().Contain(t => t.Key == "channel.name");
        }

        [Fact]
        public async Task ProcessMessagesAsync_ShouldCreateConsumerActivity()
        {
            // Arrange
            var activities = new List<Activity>();
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("Channel.Processing")
                .AddInMemoryExporter(activities)
                .Build();

            var channel = new ObservableChannelWithTracing<string>("test-channel");
            await channel.PublishAsync("test message");

            var processed = false;
            var cts = new CancellationTokenSource();

            // Act
            var processTask = channel.ProcessMessagesAsync(async (msg, ct) =>
            {
                await Task.Delay(10, ct);
                processed = true;
                cts.Cancel(); // Stop after first message
            }, cts.Token);

            try
            {
                await processTask;
            }
            catch (OperationCanceledException) { }

            tracerProvider.ForceFlush();

            // Assert
            processed.Should().BeTrue();
            activities.Should().Contain(a => a.OperationName == "channel.process");
            var activity = activities.First(a => a.OperationName == "channel.process");
            activity.Kind.Should().Be(ActivityKind.Consumer);
            activity.Status.Should().Be(ActivityStatusCode.Ok);
        }

        [Fact]
        public async Task ProcessMessagesAsync_WhenHandlerFails_ShouldRecordError()
        {
            // Arrange
            var activities = new List<Activity>();
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("Channel.Processing")
                .AddInMemoryExporter(activities)
                .Build();

            var channel = new ObservableChannelWithTracing<string>("test-channel");
            await channel.PublishAsync("test message");

            var cts = new CancellationTokenSource();

            // Act
            var processTask = channel.ProcessMessagesAsync(async (msg, ct) =>
            {
                await Task.Delay(10, ct);
                cts.Cancel(); // Stop processing
                throw new InvalidOperationException("Test error");
            }, cts.Token);

            try
            {
                await processTask;
            }
            catch (OperationCanceledException) { }

            tracerProvider.ForceFlush();

            // Assert
            var activity = activities.FirstOrDefault(a => a.OperationName == "channel.process");
            activity.Should().NotBeNull();
            activity!.Status.Should().Be(ActivityStatusCode.Error);
            activity.Events.Should().Contain(e => e.Name == "exception");
        }
    }
}
