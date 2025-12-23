using BlazorRedux.Web;
using BlazorRedux.Web.Components;
using BlazorRedux.Web.Workers;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using SolaceOboManager.Benchmarks.Solace;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddSolace();
builder.Services.AddHostedService<BlazorRedux.Web.Workers.Worker>();

builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);

#if DEBUG
    options.UseReduxDevTools(config =>
    {
        config.EnableStackTrace();
    }); // Enable for debugging state changes
#endif
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    ;

builder.Services.AddOutputCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
