using MyApp;
using MyApp.Modules.Orders;
using MyApp.Modules.Customers;
using MyApp.Modules.Billing;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOrdersModule()
    .AddCustomersModule()
    .AddBillingModule();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
