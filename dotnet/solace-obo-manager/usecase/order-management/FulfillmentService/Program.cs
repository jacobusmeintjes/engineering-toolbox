using FulfillmentService.Repositories;
using FulfillmentService.Services;
using FulfilmentService.Consumers;
using Messaging.Events.Inventory;
using Microsoft.EntityFrameworkCore;
using OrderService.HttpClients;
using static Messaging.MessagingServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // add this

builder.Services.AddDbContext<FulfilmentDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("FulfilmentDb")));

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddSingleton<IWarehouseSystem, WarehouseSystem>();
builder.Services.AddSingleton<ICarrierService, CarrierService>();

// Solace
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddSubscriber<StockReserved, StockReservedConsumer>();
builder.Services.AddHostedService<StockReservedSubscriber>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FulfilmentDbContext>();
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
