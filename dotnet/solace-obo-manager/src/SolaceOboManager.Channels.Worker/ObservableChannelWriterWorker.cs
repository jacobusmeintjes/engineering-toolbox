using SolaceOboManager.Shared.Channels;

namespace SolaceOboManager.Channels.Worker;

public class ObservableChannelWriterWorker(ILogger<ObservableChannelWriterWorker> logger, ObservableChannel<string> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Arrange
        //var channel = new ObservableChannel<string>("test-channel", capacity: 2);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await channel.PublishAsync("test message");
            await Task.Delay(100, stoppingToken);
        }
    }
}


public class ObservableChannelReaderWorker(ILogger<ObservableChannelReaderWorker> logger, ObservableChannel<string> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Arrange
        //var channel = new ObservableChannel<string>("test-channel", capacity: 2);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            while (await channel.Reader.WaitToReadAsync())
            {
                if (channel.Reader.TryRead(out var envelope))
                {
                   // received.Add(envelope.Message);
                     logger.LogInformation("Received message: {message} at {time}", envelope.Message, DateTimeOffset.Now);
                    await Task.Delay(20, stoppingToken);
                    channel.RecordProcessed(envelope, 20);
                }
               // if (received.Count == messages.Length) break;
            }

            await Task.Delay(150, stoppingToken);
        }
    }
}
