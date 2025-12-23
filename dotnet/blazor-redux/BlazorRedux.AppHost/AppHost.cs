using Aspire.Hosting;
using SolaceOboManager.Aspire;
using SolaceOboManager.Aspire.Model;
using ToxiProxy.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var solace = builder.AddSolace("solace")
    .WithSolaceVolume()    
    .WithLifetime(ContainerLifetime.Persistent);

solace.Resource.AddClientProfile(new ClientProfile("clientProfile", true, 2000));

solace.Resource.AddAclProfile(new AclProfile("clientProfile", "allow"));

solace.Resource.AddPublishTopicException(new PublishTopicException("clientProfile", "subscriptionRequest"));

solace.Resource.AddUser(new ClientUser("obomanager", "password", true));

solace.Resource.AddUser(new ClientUser("client", "password", false, "clientProfile", "clientProfile"));

solace.Resource.AddUser(new ClientUser("publisher", "password"));


var toxiproxy = builder.AddToxiProxy("toxiproxy")
        .WithEnvironment("NO_PROXY", "localhost,127.0.0.1");
        //.WithReference(solace, "tcp", "solace_tcp");


// Add a chaos control service
var chaosController = builder.AddProject<Projects.ChaosController>("chaos-controller")
    .WithReference(toxiproxy)    
    .WaitFor(toxiproxy)
    .WaitFor(solace);

var apiService = builder.AddProject<Projects.BlazorRedux_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BlazorRedux_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(solace)
    .WaitFor(apiService)
    .WaitFor(solace);

//builder.AddProject<Projects.BlazorFluxor_App>("blazorfluxorapp");

builder.Build().Run();
