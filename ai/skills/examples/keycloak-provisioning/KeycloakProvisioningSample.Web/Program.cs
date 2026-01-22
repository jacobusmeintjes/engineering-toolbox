using KeycloakProvisioningSample.Web;
using KeycloakProvisioningSample.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure Keycloak authentication
var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]?.TrimEnd('/');
var keycloakRealm = builder.Configuration["Keycloak:Realm"];
var keycloakClientId = builder.Configuration["Keycloak:ClientId"] ?? "acme-corp";

if (!string.IsNullOrWhiteSpace(keycloakBaseUrl) && !string.IsNullOrWhiteSpace(keycloakRealm))
{
    var authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = authority;
        options.ClientId = keycloakClientId;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = "realm_access.roles";

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.RedirectUri = context.Request.Scheme + "://" + context.Request.Host + "/signin-oidc";
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add authentication endpoints
app.MapGet("/Account/ExternalLogin", async (HttpContext context, string? returnUrl) =>
{
    var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/"
    };
    
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authProperties);
}).AllowAnonymous();

app.MapGet("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = "/"
    });
}).RequireAuthorization();

app.MapDefaultEndpoints();

app.Run();
