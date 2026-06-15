// Oms.Messaging.SmokeTest/Program.cs
using Messaging.Abstractions;
using Messaging.Events.Orders;
using Messaging.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Messaging;
using Messaging.Events.Orders;
using Messaging.Topics;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddMessaging(ctx.Configuration);
    })
    .Build();

var publisher = host.Services.GetRequiredService<IEventPublisher>();
var connection = host.Services.GetRequiredService<SolaceConnection>();

await connection.InitialiseAsync(CancellationToken.None);

var @event = new OrderPlaced
{
    OrderId = Guid.NewGuid(),
    CustomerId = Guid.NewGuid(),
    ShippingAddress = "1 Test Street, Cape Town",
    PaymentMethodToken = "tok_test_123",
    TotalAmount = 299.99m,
    Items = new List<Contracts.Domain.OrderItem>
    {
        new(Guid.NewGuid(), "SKU-001", "Test Widget", 2, 149.99m)
    }
};

await publisher.PublishAsync(@event, Topics.Orders.Placed, CancellationToken.None);

Console.WriteLine($"Published OrderPlaced event {@event.EventId}");
Console.WriteLine("Check Solace Manager at http://localhost:8080 to verify");