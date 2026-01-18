using keycloak.AppHost.Keycloak.Model;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace keycloak.AppHost.Keycloak
{
    public static class KeycloakExtensions
    {
        public static IResourceBuilder<KeycloakResource> AddKeycloak(this IDistributedApplicationBuilder builder)
        {
            var username = builder.AddParameter("username", "admin", false);
            var password = builder.AddParameter("password", "Password@123", secret: true);

            var keycloak = builder.AddKeycloak("keycloak", 8080, username, password)
                .WithEnvironment("KC_LOG_LEVEL", "DEBUG")
                .WithEnvironment("KEYCLOAK_LOGLEVEL", "DEBUG")
                .WithEnvironment("ROOT_LOGLEVEL", "DEBUG")
                .WithDataVolume()
                .WithOtlpExporter()
                .WithLifetime(ContainerLifetime.Persistent);

            builder.Eventing.Subscribe<ResourceReadyEvent>(keycloak.Resource, async (@event, cancellationToken) =>
            {
                var eventServices = new ServiceCollection();
                if (@event.Resource.TryGetEndpoints(out var endpoints) && @event.Resource.TryGetEnvironmentVariables(out var environmentVariables))
                {

                    var adminEndpoint = endpoints.FirstOrDefault(c => c.Name == "http");

                    if (adminEndpoint == null)
                        return;

                    if (adminEndpoint.AllocatedEndpoint == null)
                        return;

                    // Usage
                    var authClient = RestService.For<IKeycloakAgent>(adminEndpoint.AllocatedEndpoint.UriString);

                    var usernameValue = await username.Resource.GetValueAsync(cancellationToken);
                    var passwordValue = await password.Resource.GetValueAsync(cancellationToken);

                    if (usernameValue == null || passwordValue == null)
                        return;

                    var request = new PasswordGrantRequest
                    {
                        ClientId = "admin-cli",
                        Username = usernameValue,
                        Password = passwordValue
                    };

                    var tokenResponse = await authClient.GetToken("master", request);

                    if (tokenResponse is null) return;

                    eventServices.AddTransient<LoggingHandler>();

                    eventServices.AddRefitClient<IKeycloakAgent>()
                       .ConfigureHttpClient(c =>
                       {
                           c.BaseAddress = new Uri(adminEndpoint.AllocatedEndpoint.UriString);
                           c.DefaultRequestHeaders.Clear();

                           c.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
                       })
                           .AddHttpMessageHandler<LoggingHandler>();


                    var eventServiceProvider = eventServices.BuildServiceProvider();

                    using var scope = eventServiceProvider.CreateScope();
                    var keycloakAgent = scope.ServiceProvider.GetRequiredService<IKeycloakAgent>();

                    var realms = await keycloakAgent.GetRealmInformation();

                    if (!realms.Any(c => c.id.Equals("developer")))
                    {
                        //Create new realm
                        await keycloakAgent.AddRealm(new RealmRequest { Realm = "developer" });
                    }
                }
            });

            return keycloak;
        }
    }
}
