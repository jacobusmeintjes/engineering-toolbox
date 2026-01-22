using SolaceOboManager.AppHost.Keycloak.Model;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace SolaceOboManager.AppHost.Keycloak
{
    public static class KeycloakExtensions
    {
        public static IResourceBuilder<KeycloakResource> AddKeycloak(this IDistributedApplicationBuilder builder, string name)
        {
            var username = builder.AddParameter("keycloak-username", "admin", false);
            var password = builder.AddParameter("keycloak-password", "Password@123", secret: true);

            var keycloak = builder.AddKeycloak(name, 8070, username, password)
                .WithEnvironment("KC_LOG_LEVEL", "DEBUG")
                .WithEnvironment("KEYCLOAK_LOGLEVEL", "DEBUG")
                .WithEnvironment("ROOT_LOGLEVEL", "DEBUG")

                //.WithDataVolume()
                .WithOtlpExporter()
                .WithLifetime(ContainerLifetime.Persistent);

            builder.Eventing.Subscribe<ResourceReadyEvent>(keycloak.Resource, async (@event, cancellationToken) =>
            {
                var eventServices = new ServiceCollection();
                if (@event.Resource.TryGetEndpoints(out var endpoints) && @event.Resource.TryGetEnvironmentVariables(out var environmentVariables))
                {

                    var adminEndpoint = endpoints.FirstOrDefault(c => c.Name == "https");

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

                    if (!realms.Any(c => c.realm.Equals("solace")))
                    {
                        //Create new realm
                        await keycloakAgent.AddRealm(new RealmRequest { Realm = "solace" });
                    }
                }
            });


            return keycloak;
        }
    }
}
