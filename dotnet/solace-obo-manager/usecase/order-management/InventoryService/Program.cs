using InventoryService.Consumer;
using InventoryService.Repositories;
using InventoryService.Services;
using Messaging.Events.Payments;
using Microsoft.EntityFrameworkCore;
using static Messaging.MessagingServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService.Services.InventoryService>();

builder.Services.AddSubscriber<PaymentAuthorised, PaymentAuthorisedConsumer>();
builder.Services.AddHostedService<PaymentAuthorisedSubscriber>();

builder.Services.AddSubscriber<PaymentFailed, PaymentFailedConsumer>();
builder.Services.AddHostedService<PaymentFailedSubscriber>();

builder.Services.AddMessaging(builder.Configuration);


// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // add this
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();
    await InventorySeeder.SeedAsync(db);
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
