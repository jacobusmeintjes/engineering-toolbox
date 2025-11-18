using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var elasticSearch = builder.AddElasticsearch("elasticsearch")
    .WithImageTag("9.1.7")    
    .WithHttpEndpoint(9200, 9200, "sec")
    .WithLifetime(ContainerLifetime.Persistent);


var kibana = builder
    .AddContainer("kibana", "elastic/kibana", "9.1.7") // Add Kibana f rom the image kibana, and tag 8.15.3 and give it a name kibana
    .WithReference(elasticSearch) // Add a reference to the elasticsearch container
    .WithHttpEndpoint(5601, 5601)
    .WithHttpHealthCheck("/status", 200, "http")
    .WaitFor(elasticSearch)
    .WithLifetime(ContainerLifetime.Persistent); // Expose a port so you can connect


var apmServer = builder.AddContainer("apm-server", "elastic/apm-server", "9.1.7")
   .WithReference(elasticSearch)
   .WaitFor(elasticSearch);

var logStash = builder.AddContainer("logstash", "elastic/logstash", "9.1.7")
   .WithReference(elasticSearch)
   .WaitFor(elasticSearch);

var elasticAgent = builder.AddContainer("elastic-agent", "elastic/elastic-agent", "9.1.7")
    .WithEnvironment("FLEET_ENROLL", "1")
   .WaitFor(elasticSearch);

var apiService = builder.AddProject<Projects.ElasticSearch_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.ElasticSearch_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("OTEL_SERVICE_NAME", "WebFrontEnd")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
