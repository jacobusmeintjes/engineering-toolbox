var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]?.TrimEnd('/');
var keycloakRealm = builder.Configuration["Keycloak:Realm"];

if (!string.IsNullOrWhiteSpace(keycloakBaseUrl) && !string.IsNullOrWhiteSpace(keycloakRealm))
{
    var authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";

    builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = false;

        // For a minimal sample, we skip audience validation. In real apps, validate audience
        // (and configure an audience mapper in Keycloak if needed).
        options.TokenValidationParameters.ValidateAudience = false;
    });

    builder.Services.AddAuthorization();
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/secure", (System.Security.Claims.ClaimsPrincipal user) =>
{
    var name = user.Identity?.Name ?? user.FindFirst("preferred_username")?.Value ?? "(unknown)";
    return Results.Ok(new
    {
        message = "Authenticated request accepted.",
        user = name,
        subject = user.FindFirst("sub")?.Value
    });
}).RequireAuthorization();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
