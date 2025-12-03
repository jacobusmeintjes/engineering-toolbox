var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("country-redis").WithRedisInsight();


var orleans = builder.AddOrleans("my-app")
                     .WithClustering(redis);
                     //.WithGrainStorage("Default", redis);


var apiService = builder.AddProject<Projects.orleans_caching_ApiService>("apiservice")
    .WithReference(orleans)
    .WithEndpoint(port:8080, name: "dashboard")
    .WaitFor(redis)
    .WithReplicas(2);

builder.AddProject<Projects.orleans_caching_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(apiService)
    .WithReference(orleans)
    .WaitFor(apiService);

builder.Build().Run();
