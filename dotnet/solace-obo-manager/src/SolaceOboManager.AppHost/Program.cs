using Microsoft.Extensions.DependencyInjection;
using SolaceOboManager.AppHost.Keycloak;
using SolaceOboManager.Aspire;
using SolaceOboManager.Aspire.Model;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("mykeycloak")
    .WithBindMount("../../certificates","/etc/x509/https", true)
    .WithEnvironment("KC_HTTPS_CERTIFICATE_FILE", "/etc/x509/https/tls.crt")
    .WithEnvironment("KC_HTTPS_CERTIFICATE_KEY_FILE", "/etc/x509/https/tls.key")
    .WithBindMount("../../data","/opt/keycloak/data");

//var cache = builder.AddRedis("cache").WithRedisInsight().WithLifetime(ContainerLifetime.Persistent);

//var username = builder.AddParameter("username", "postgres", secret: false);

//var password = builder.AddParameter("password", "password123!", secret: false);

//var postgres = builder.AddPostgres("postgres", username, password)
//    .WithPgAdmin()
//    .WithLifetime(ContainerLifetime.Persistent);

builder.Configuration["DcpPublisher:RandomizePorts"] = "false";

var solace = builder.AddSolace("solace", true)
    .WithEnvironment("tls_servercertificate_filepath", "/var/run/secrets/mypubsub.pem")
    .WithBindMount("../../certificates/mypubsub.pem", "/var/run/secrets/mypubsub.pem", true)
    .WithBindMount("../../logs", "/var/lib/solace/jail/logs")
    //.WithSolaceVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(keycloak);

solace.Resource.AddClientProfile(new ClientProfile("clientProfile", true, 2000));
solace.Resource.AddAclProfile(new AclProfile("clientProfile", "allow"));
solace.Resource.AddPublishTopicException(new PublishTopicException("clientProfile", "subscriptionRequest"));
solace.Resource.AddUser(new ClientUser("obomanager", "password", true));
solace.Resource.AddUser(new ClientUser("client", "password", false, "clientProfile", "clientProfile"));
solace.Resource.AddUser(new ClientUser("publisher", "password"));

//builder.AddProject<Projects.SolaceOboManager_Manager>("solaceobomanager-manager")
//    .WithEnvironment("SolaceConfiguration__VPNName", "default")
//    .WithEnvironment("SolaceConfiguration__Username", "obomanager")
//    .WithEnvironment("SolaceConfiguration__Password", "password")
//    .WithReference(solace)
//    .WaitFor(solace)
//    .WithReplicas(1);

//builder.AddProject<Projects.SolaceOboManager_Client>("solaceobomanager-client")
//    .WithReference(solace)
//    .WaitFor(solace)
//    .WithExplicitStart();

builder.AddProject<Projects.SolaceOboManager_OAuthClient>("solaceobomanager-oauth-client")
    .WithReference(keycloak)
    .WithReference(solace)
    .WaitFor(keycloak)
    .WaitFor(solace);

//builder.AddProject<Projects.SolaceOboManager_Producer>("solaceobomanager-producer")
//    .WithReference(solace)
//    .WaitFor(solace)
//    .WithExplicitStart()
//    .WithReplicas(1);

//builder.AddProject<Projects.SolaceOboManager_Channels_Worker>("solaceobomanager-channels-worker")
//    .WithReference(solace)
//    .WaitFor(solace);

builder.Build().Run();
