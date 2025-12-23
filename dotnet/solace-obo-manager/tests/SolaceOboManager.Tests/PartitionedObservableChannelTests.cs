using FluentAssertions;
using SolaceOboManager.Shared.Channels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SolaceOboManager.Tests
{
    public class PartitionedObservableChannelTests
    {
        [Fact]
        public async Task PublishAsync_ShouldDistributeToPartitions()
        {
            // Arrange
            var channel = new PartitionedObservableChannel<string, TestMessage>(
                "test-channel",
                partitionCount: 4,
                keySelector: msg => msg.CustomerId);

            var messages = new[]
            {
            new TestMessage("customer-1", "order-1"),
            new TestMessage("customer-1", "order-2"),
            new TestMessage("customer-2", "order-3"),
            new TestMessage("customer-3", "order-4"),
        };

            // Act
            foreach (var msg in messages)
            {
                await channel.PublishAsync(msg);
            }

            // Assert - Messages with same key should go to same partition
            // This is implicitly tested by processing them
            var processedByCustomer = new Dictionary<string, List<string>>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var tasks = channel.StartProcessingAsync(async (msg, ct) =>
            {
                lock (processedByCustomer)
                {
                    if (!processedByCustomer.ContainsKey(msg.CustomerId))
                    {
                        processedByCustomer[msg.CustomerId] = new List<string>();
                    }
                    processedByCustomer[msg.CustomerId].Add(msg.OrderId);
                }
                await Task.Delay(10, ct);
            }, cts.Token);

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }

            processedByCustomer["customer-1"].Should().HaveCount(2);
            processedByCustomer["customer-2"].Should().HaveCount(1);
            processedByCustomer["customer-3"].Should().HaveCount(1);
        }

        [Fact]
        public async Task PublishAsync_SameKey_ShouldMaintainOrder()
        {
            // Arrange
            var channel = new PartitionedObservableChannel<string, TestMessage>(
                "test-channel",
                partitionCount: 4,
                keySelector: msg => msg.CustomerId);

            var messages = Enumerable.Range(1, 100)
                .Select(i => new TestMessage("customer-1", $"order-{i}"))
                .ToArray();

            // Act
            foreach (var msg in messages)
            {
                await channel.PublishAsync(msg);
            }

            // Assert - Should maintain order for same customer
            var receivedOrders = new List<string>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var tasks = channel.StartProcessingAsync(async (msg, ct) =>
            {
                lock (receivedOrders)
                {
                    receivedOrders.Add(msg.OrderId);
                }
                await Task.Delay(1, ct);
            }, cts.Token);

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }

            receivedOrders.Should().Equal(messages.Select(m => m.OrderId));
        }

        [Fact]
        public async Task StartProcessingAsync_ShouldProcessInParallel()
        {
            // Arrange
            var channel = new PartitionedObservableChannel<string, TestMessage>(
                "test-channel",
                partitionCount: 4,
                keySelector: msg => msg.CustomerId);

            // Publish to different partitions
            var customers = new[] { "customer-1", "customer-2", "customer-3", "customer-4" };
            foreach (var customer in customers)
            {
                await channel.PublishAsync(new TestMessage(customer, "order-1"));
            }

            var list = new List<string>();

            // Act
            var processingStartTimes = new ConcurrentDictionary<string, long>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var tasks = channel.StartProcessingAsync(async (msg, ct) =>
            {
                var timestamp = Stopwatch.GetTimestamp();
                processingStartTimes.TryAdd(msg.CustomerId, timestamp);
                list.Add(Thread.CurrentThread.ManagedThreadId.ToString());
                await Task.Delay(10, ct); // Simulate work
            }, cts.Token);

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }

            // Assert - All should start processing roughly at the same time
            var times = processingStartTimes.Values.ToArray();
            var maxDiff = times.Max() - times.Min();
            var maxDiffMs = maxDiff / (double)Stopwatch.Frequency * 1000;

            maxDiffMs.Should().BeLessThan(50); // Started within 50ms of each other

            list.Should().HaveCount(customers.Length);
        }

        private record TestMessage(string CustomerId, string OrderId);
    }
}
