using FluentAssertions;
using SolaceOboManager.Shared.Channels;

namespace SolaceOboManager.Tests
{
    public class ObservableChannelTests
    {
        [Fact]
        public async Task PublishAsync_ShouldEnqueuMessage()
        {
            //Arrange
            var channel = new ObservableChannel<string>("test-channel");
            var message = "Test Message";

            //Act
            await channel.PublishAsync(message);

            //Assert
            var envelope = await channel.Reader.ReadAsync();
            Assert.Equal(message, envelope.Message);
        }


        [Fact]
        public async Task PublishAsync_MultipleMessages_ShouldMaintainOrder()
        {
            // Arrange
            var channel = new ObservableChannel<int>("test-channel");
            var messages = Enumerable.Range(1, 100).ToArray();

            // Act
            foreach (var msg in messages)
            {
                await channel.PublishAsync(msg);
            }

            // Assert
            var received = new List<int>();
            while (await channel.Reader.WaitToReadAsync())
            {
                if (channel.Reader.TryRead(out var envelope))
                {
                    received.Add(envelope.Message);
                }
                if (received.Count == messages.Length) break;
            }

            received.Should().Equal(messages);
        }

        [Fact]
        public async Task PublishAsync_WhenChannelFull_ShouldWaitForCapacity()
        {
            // Arrange
            var channel = new ObservableChannel<string>("test-channel", capacity: 2);

            // Act - Fill the channel
            await channel.PublishAsync("msg1");
            await channel.PublishAsync("msg2");

            // This should wait but complete once we read
            var publishTask = channel.PublishAsync("msg3");

            // Verify it's waiting
            await Task.Delay(100);
            publishTask.IsCompleted.Should().BeFalse();

            // Read one message to free up space
            await channel.Reader.ReadAsync();
            await Task.Delay(100);
            // Now publish should complete
            //await publishTask.WaitAsync(TimeSpan.FromSeconds(1));
            publishTask.IsCompleted.Should().BeTrue();
        }
    }
}
