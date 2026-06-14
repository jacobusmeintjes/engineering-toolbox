using Microsoft.Extensions.DependencyInjection;
using SolaceOboManager.Aspire;
using SolaceOboManager.Aspire.Model;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight(options=>
    {
        options.WithHostPort(16379);
        options.WithLifetime(ContainerLifetime.Persistent);
    })
    .WithLifetime(ContainerLifetime.Persistent);

var username = builder.AddParameter("username", "postgres", secret: false);

var password = builder.AddParameter("password", "password123!", secret: false);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithPgAdmin(options =>
    {
        options.WithHostPort(15432);
        options.WithLifetime(ContainerLifetime.Persistent);
    })
    .WithLifetime(ContainerLifetime.Persistent);

var orderDb = postgres.AddDatabase("OrderDb");
var paymentDb = postgres.AddDatabase("PaymentDb");
var inventoryDb = postgres.AddDatabase("InventoryDb");
var fulfilmentDb = postgres.AddDatabase("FulfilmentDb");
var notificationDb = postgres.AddDatabase("NotificationDb");

builder.Configuration["DcpPublisher:RandomizePorts"] = "false";

var solace = builder.AddSolace("solace")
    .WithSolaceVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithExplicitStart();

solace.Resource.AddClientProfile(new ClientProfile("clientProfile", true, 2000));
solace.Resource.AddAclProfile(new AclProfile("clientProfile", "allow"));
solace.Resource.AddPublishTopicException(new PublishTopicException("clientProfile", "subscriptionRequest"));
solace.Resource.AddUser(new ClientUser("obomanager", "password", true));
solace.Resource.AddUser(new ClientUser("client", "password", false, "clientProfile", "clientProfile"));
solace.Resource.AddUser(new ClientUser("publisher", "password"));

builder.AddProject<Projects.SolaceOboManager_Manager>("solaceobomanager-manager")
    .WithEnvironment("SolaceConfiguration__VPNName", "default")
    .WithEnvironment("SolaceConfiguration__Username", "obomanager")
    .WithEnvironment("SolaceConfiguration__Password", "password")
    .WithReference(solace)
    .WaitFor(solace)
    .WithReplicas(1);

builder.AddProject<Projects.SolaceOboManager_Client>("solaceobomanager-client")
    .WithReference(solace)
    .WaitFor(solace)
    .WithExplicitStart();

builder.AddProject<Projects.SolaceOboManager_Producer>("solaceobomanager-producer")
    .WithReference(solace)
    .WaitFor(solace)
    .WithExplicitStart()
    .WithReplicas(1);

builder.AddProject<Projects.SolaceOboManager_Channels_Worker>("solaceobomanager-channels-worker")
    .WithReference(solace)
    .WaitFor(solace)
    .WithExplicitStart();


var paymentService = builder.AddProject<Projects.PaymentService>("paymentservice")
    .WithReference(paymentDb, "PaymentDb")
    .WaitFor(paymentDb);

var inventoryService = builder.AddProject<Projects.InventoryService>("inventoryservice")
    .WithReference(inventoryDb, "InventoryDb")
    .WaitFor(inventoryDb);

var fulfilmentService = builder.AddProject<Projects.FulfilmentService>("fulfilmentservice")
    .WithReference(fulfilmentDb, "FulfilmentDb")
    .WaitFor(fulfilmentDb);

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
    .WithReference(notificationDb, "NotificationDb")
    .WaitFor(notificationDb);

var orderService = builder.AddProject<Projects.OrderService>("orderservice")
    .WithReference(orderDb, "OrderDb")
    .WithReference(paymentService)
    .WithReference(inventoryService)
    .WithReference(fulfilmentService)
    .WithReference(notificationService)
    .WaitFor(orderDb)
    .WaitFor(paymentService)
    .WaitFor(inventoryService)
    .WaitFor(fulfilmentService)
    .WaitFor(notificationService);

builder.Build().Run();
