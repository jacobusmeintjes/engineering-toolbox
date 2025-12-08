namespace SolaceOboManager.IntegrationTests
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SolaceOboManager.Shared.Channels;

    public class ChannelBackgroundServiceTests
    {
        [Fact]
        public async Task BackgroundService_ShouldProcessMessages()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<ObservableChannelWithTracing<TestOrder>>(
                sp => new ObservableChannelWithTracing<TestOrder>("test-orders"));
            services.AddSingleton<IOrderService, FakeOrderService>();
            services.AddHostedService<OrderProcessor>();

            var serviceProvider = services.BuildServiceProvider();
            var channel = serviceProvider.GetRequiredService<ObservableChannelWithTracing<TestOrder>>();
            var orderService = serviceProvider.GetRequiredService<IOrderService>() as FakeOrderService;

            // Start the hosted service
            var hostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<OrderProcessor>()
                .First();

            await hostedService.StartAsync(CancellationToken.None);

            // Act
            var order = new TestOrder(Guid.NewGuid(), "customer-1", 100m);
            await channel.PublishAsync(order);

            // Wait for processing
            await Task.Delay(500);

            // Assert
            orderService!.ProcessedOrders.Should().ContainSingle();
            orderService.ProcessedOrders[0].Id.Should().Be(order.Id);

            // Cleanup
            await hostedService.StopAsync(CancellationToken.None);
        }

        private record TestOrder(Guid Id, string CustomerId, decimal Amount);

        private class FakeOrderService : IOrderService
        {
            public List<TestOrder> ProcessedOrders { get; } = new();

            public Task ProcessAsync(object order, CancellationToken ct)
            {
                if (order is TestOrder testOrder)
                {
                    ProcessedOrders.Add(testOrder);
                }
                return Task.CompletedTask;
            }
        }
    }


    public class OrderProcessor : BackgroundService
    {
        private readonly ObservableChannelWithTracing<OrderMessage> _channel;
        private readonly ILogger<OrderProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OrderProcessor(
            ObservableChannelWithTracing<OrderMessage> channel,
            ILogger<OrderProcessor> logger,
            IServiceProvider serviceProvider)
        {
            _channel = channel;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _channel.ProcessMessagesAsync(ProcessOrderAsync, stoppingToken);
        }

        private async Task ProcessOrderAsync(OrderMessage order, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            _logger.LogInformation("Processing order {OrderId}", order.Id);

            await orderService.ProcessAsync(order, ct);
        }
    }

    public record OrderMessage(Guid Id, string CustomerId, decimal Amount);


    public interface IOrderService
    {
        Task ProcessAsync(object o, CancellationToken ct);
    }
}
