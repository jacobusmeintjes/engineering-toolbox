using Aspire.Hosting;
using sonarqube_codecharta.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sonarQube = builder.AddSonarQube("admin", "admin", "Password123!");

builder.AddContainer("codecharta-visualization", "codecharta/codecharta-visualization")
    .WithHttpEndpoint(targetPort: 80);

//var test = builder.AddDockerfile("test", "./", "dockerfile")
//    .WithBindMount("../../containers/test", "/data/output")
//    .WithContainerName("test");

var test = builder.AddContainer("test", "test", "latest")
    .WithBindMount("../../containers/test", "/data/output")
    .WithContainerName("test")
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.sonarqube_codecharta_ApiService>("apiservice");

builder.AddProject<Projects.sonarqube_codecharta_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithExplicitStart()
    .WithSonarQubeAnalyzer(sonarQube, false)
    .WithCodeChartaAnalyzer(test);

builder.Build().Run();
