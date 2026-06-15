using Messaging.Events.Inventory;
using Messaging.Events.Orders;
using Microsoft.EntityFrameworkCore;
using OrderService.HttpClients;
using PaymentService.Consumers;
using PaymentService.Repositories;
using PaymentService.Services;
using static Messaging.MessagingServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDb")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // add this

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Solace — consumers run as hosted background services
builder.Services.AddMessaging(builder.Configuration);

builder.Services.AddSubscriber<OrderPlaced, OrderPlacedConsumer>();
builder.Services.AddHostedService<OrderPlacedSubscriber>();

builder.Services.AddSubscriber<StockReservationFailed, StockReservationFailedConsumer>();
builder.Services.AddHostedService<StockReservationFailedSubscriber>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.MigrateAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
