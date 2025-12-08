using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using SolaceOboManager.AdminService.SolaceConfig;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithRedisInsight().WithLifetime(ContainerLifetime.Persistent);

var username = builder.AddParameter("username", "postgres", secret: false);

var password = builder.AddParameter("password", "password123!", secret: false);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

builder.Configuration["DcpPublisher:RandomizePorts"] = "false";

//var solace = builder.AddSolace("solace");
//.WithLifetime(ContainerLifetime.Persistent);

var solace = builder.AddContainer("pubSubStandardSingleNode", "solace/solace-pubsub-standard", "latest")
    .WithVolume("storage-group", "/var/lib/solace")
    .WithContainerRuntimeArgs("--shm-size", "1g", "--ulimit", "core=-1", "--ulimit", "nofile=2448:6592")
    .WithEnvironment("system_scaling_maxconnectioncount", "100")
    .WithEnvironment("username_admin_password", "admin")
    .WithEnvironment("username_admin_globalaccesslevel", "admin")
    .WithHttpEndpoint(8080, 8080, "admin")
    .WithEndpoint(8008, 8008, "ws")
    .WithEndpoint(8000, 8000, "mqtt")
    .WithEndpoint(port: 15555, targetPort: 55555, scheme: "tcp", name: "bus")
    .WithHttpEndpoint(5550, 5550, "health")
    .WithHttpHealthCheck("/health-check/direct-active", statusCode: 200, endpointName: "health")
    .WithLifetime(ContainerLifetime.Persistent);


builder.Eventing.Subscribe<ResourceReadyEvent>(solace.Resource, async (@event, cancellationToken) =>
{
    var eventServices = new ServiceCollection();

    if (@event.Resource.TryGetEndpoints(out var endpoints) && @event.Resource.TryGetEnvironmentVariables(out var environmentVariables))
    {
        var adminEndpoint = endpoints.FirstOrDefault(c => c.Name == "admin");

        eventServices.AddRefitClient<ISolaceConfigurationAgent>()
           .ConfigureHttpClient(c =>
           {
               c.BaseAddress = new Uri(adminEndpoint.AllocatedEndpoint.UriString);
               c.DefaultRequestHeaders.Clear();
               var authenticationString = "admin:admin";
               var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
               c.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
           });

        var eventServiceProvider = eventServices.BuildServiceProvider();

        using var scope = eventServiceProvider.CreateScope();
        var solaceClient = scope.ServiceProvider.GetRequiredService<ISolaceConfigurationAgent>();
        try
        {
            await solaceClient.CreateClientProfile("default", new MsgVpnClientProfile { ClientProfileName = "clientProfile", ElidingEnabled = false, ElidingDelay = 2000 });

            await solaceClient.CreateAclProfile("default", new MsgVpnAclProfile { AclProfileName = "clientProfile", ClientConnectDefaultAction = "allow" });
            await solaceClient.CreatePublishTopicExceptions("default", "clientProfile", new MsgVpnAclProfilePublishTopicException { AclProfileName = "clientProfile", VpnName = "default", PublishTopicException = "subscriptionRequest" });

            await solaceClient.CreateUser("default", new MsgVpnClientUsername { Username = "obomanager", Password = "password", SubscriptionManagerEnabled = true });
            await solaceClient.CreateUser("default", new MsgVpnClientUsername { Username = "client", Password = "password", AclProfileName = "clientProfile", ProfileName = "clientProfile" });
            await solaceClient.CreateUser("default", new MsgVpnClientUsername { Username = "publisher", Password = "password" });
        }
        catch { }
    }
});

builder.AddProject<Projects.SolaceOboManager_Manager>("solaceobomanager-manager")
    .WithEnvironment("SolaceConfiguration__VPNName", "default")
    .WithEnvironment("SolaceConfiguration__Username", "obomanager")
    .WithEnvironment("SolaceConfiguration__Password", "password")
    .WaitFor(solace)
    .WithReplicas(1);

builder.AddProject<Projects.SolaceOboManager_Client>("solaceobomanager-client")
    .WaitFor(solace)
    .WithExplicitStart();

builder.AddProject<Projects.SolaceOboManager_Producer>("solaceobomanager-producer")
    .WaitFor(solace)
    .WithExplicitStart()
    .WithReplicas(1);

builder.AddProject<Projects.SolaceOboManager_Channels_Worker>("solaceobomanager-channels-worker");

builder.Build().Run();
