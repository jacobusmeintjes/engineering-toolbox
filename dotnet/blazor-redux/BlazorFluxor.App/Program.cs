using BlazorFluxor.App.Components;
using BlazorFluxor.App.Store;
using BlazorFluxor.App.Worker;
using Fluxor;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Clear default providers and add what you want
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Set minimum level
builder.Logging.SetMinimumLevel(LogLevel.Trace);


builder.Services.AddSingleton<CounterService>();
// Or configure specific categories
//builder.Logging.AddFilter("Fluxor", LogLevel.Debug);
builder.Services.AddHostedService<CounterWorker>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluxor(options => {     
    options.ScanAssemblies(typeof(CounterState).Assembly);
    
});

var app = builder.Build();


app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();




app.Run();
