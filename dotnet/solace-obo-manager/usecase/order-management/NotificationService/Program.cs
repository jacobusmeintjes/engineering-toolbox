using Messaging;
using Messaging.Events.Fulfilment;
using Messaging.Events.Payments;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Repositories;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // add this

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<NotificationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDb")));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<ISmsGateway, SmsGateway>();
builder.Services.AddSingleton<ICustomerResolver, CustomerResolver>();


builder.Services.AddMessaging(builder.Configuration);


builder.Services.AddSubscriber<ShipmentCreated, ShipmentCreatedConsumer>();
builder.Services.AddHostedService<ShipmentCreatedSubscriber>();

builder.Services.AddSubscriber<ShipmentShipped, ShipmentShippedConsumer>();
builder.Services.AddHostedService<ShipmentShippedSubscriber>();

builder.Services.AddSubscriber<ShipmentDelivered, ShipmentDeliveredConsumer>();
builder.Services.AddHostedService<ShipmentDeliveredSubscriber>();

builder.Services.AddSubscriber<PaymentFailed, PaymentFailedConsumer>();
builder.Services.AddHostedService<PaymentFailedSubscriber>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
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
