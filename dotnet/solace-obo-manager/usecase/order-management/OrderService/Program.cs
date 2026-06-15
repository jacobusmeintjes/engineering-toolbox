using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OrderService.Exceptions;
using OrderService.HttpClients;
using OrderService.Repositories;
using OrderService.Services;
using static Messaging.MessagingServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// EF Core
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb")));


builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<RestOrchestrator>();

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<EventDrivenOrderOrchestrator>();

// Typed HTTP clients — each pointing at its own service
var paymentServiceUri = builder.Configuration["Services:PaymentService:HTTPS:0"]!;
var inventoryServiceUri = builder.Configuration["Services:InventoryService:HTTPS:0"]!;
var fulfilmentServiceUri = builder.Configuration["Services:FulfilmentService:HTTPS:0"]!;
var notificationServiceUri = builder.Configuration["Services:NotificationService:HTTPS:0"]!;

builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(c =>
    c.BaseAddress = new Uri(paymentServiceUri));

builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(c =>
    c.BaseAddress = new Uri(inventoryServiceUri));

builder.Services.AddHttpClient<IFulfilmentClient, FulfilmentClient>(c =>
    c.BaseAddress = new Uri(fulfilmentServiceUri));

builder.Services.AddHttpClient<INotificationClient, NotificationClient>(c =>
    c.BaseAddress = new Uri(notificationServiceUri));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

// Program.cs addition
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();

    (int status, string detail) = feature?.Error switch
    {
        PaymentFailedException ex => (402, ex.Message),
        InsufficientStockException ex => (409, ex.Message),
        _ => (500, "An unexpected error occurred")
    };

    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new { detail });
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
