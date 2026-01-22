using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using SolaceOboManager.Aspire.Solace;

namespace SolaceOboManager.Aspire
{
    public static class DistributedApplicationBuilderExtensions
    {
        public static IResourceBuilder<SolaceResource> AddSolace(this IDistributedApplicationBuilder? builder,
            string name, bool is_secure = false)
        {
            var solaceResource = new SolaceResource(name);

            var built = builder.AddResource(solaceResource)
                .WithImage(SolaceResource.DefaultImage)
                .WithImageRegistry(SolaceResource.DefaultRegistry)
                .WithImageTag(SolaceResource.DefaultTag)
                .WithContainerRuntimeArgs("--shm-size", "2g", "--ulimit", "core=-1", "--ulimit", "nofile=2448:6592")
                .WithEnvironment("system_scaling_maxconnectioncount", "100")
                .WithEnvironment("username_admin_password", solaceResource.AdminPasswordParameter)
                .WithEnvironment("username_admin_globalaccesslevel", "admin")                                
                .WithEndpoint(8008, 8008, "ws")
                .WithEndpoint(8000, 8000, "mqtt")                               
                .WithHttpEndpoint(5550, 5550, "health")
                .WithHttpHealthCheck("/health-check/direct-active", statusCode: 200, endpointName: "health");

            if (!is_secure)
            {
                built.WithHttpEndpoint(8080, 8080, "admin")
                    .WithEndpoint(port: 15555, targetPort: 55555, scheme: "tcp", name: "host");
            }
            else
            {
                built.WithEndpoint(port: 55443, targetPort: 55443, scheme: "tcps", name: "host")
                    .WithEndpoint(port: 1943, targetPort: 1943, scheme: "https", name: "admin");
            }

            var resource = solaceResource;
            var adminUsername = resource.AdminUsername;
            var adminPassword = resource.AdminPasswordParameter;

            var clientProfiles = resource.ClientProfiles;


            builder.Eventing.Subscribe<ResourceReadyEvent>(solaceResource, async (@event, cancellationToken) =>
            {
                if (resource.AclProfiles.Any() || resource.PublishTopicExceptions.Any() || resource.Users.Any() || resource.ClientProfiles.Any())
                {
                    var eventServices = new ServiceCollection();

                    if (@event.Resource.TryGetEndpoints(out var endpoints) && @event.Resource.TryGetEnvironmentVariables(out var environmentVariables))
                    {

                        var adminEndpoint = endpoints.FirstOrDefault(c => c.Name == "admin");

                        if (adminEndpoint == null)
                            return;

                        if (adminEndpoint.AllocatedEndpoint == null)
                            return;

                        eventServices.AddRefitClient<ISolaceConfigurationAgent>()
                           .ConfigureHttpClient(c =>
                           {
                               c.BaseAddress = new Uri(adminEndpoint.AllocatedEndpoint.UriString);
                               c.DefaultRequestHeaders.Clear();
                               var authenticationString = $"{adminUsername}:{adminPassword.GetValueAsync(default).Result}";
                               var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
                               c.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
                           });

                        var eventServiceProvider = eventServices.BuildServiceProvider();

                        using var scope = eventServiceProvider.CreateScope();
                        var solaceClient = scope.ServiceProvider.GetRequiredService<ISolaceConfigurationAgent>();
                        try
                        {
                            foreach (var clientProfile in resource.ClientProfiles)
                            {
                                await solaceClient.CreateClientProfile("default", clientProfile);
                            }

                            foreach (var aclProfile in resource.AclProfiles)
                            {
                                await solaceClient.CreateAclProfile("default", aclProfile);
                            }

                            foreach (var publishTopicException in resource.PublishTopicExceptions)
                            {
                                await solaceClient.CreatePublishTopicExceptions("default", publishTopicException.AclProfileName, publishTopicException);
                            }

                            foreach (var user in resource.Users)
                            {
                                await solaceClient.CreateUser("default", user);
                            }
                        }
                        catch { }
                    }
                }
            });

            return built;
        }
    }
}
