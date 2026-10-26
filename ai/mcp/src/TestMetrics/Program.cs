using TestMetrics.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddSingleton<VehicleStore>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
