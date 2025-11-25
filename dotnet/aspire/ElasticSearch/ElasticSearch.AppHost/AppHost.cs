using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var elasticSearch = builder.AddElasticsearch("elasticsearch", port: 19200)
    .WithImageTag("9.1.7")
    .WithEnvironment("xpack.security.enabled", "true") // MUST be enabled for Fleet
    .WithEnvironment("xpack.security.authc.api_key.enabled", "true") // Enable API keys for Fleet
    .WithEnvironment("ELASTIC_PASSWORD", "changeme") // Password for elastic user
    .WithEnvironment("discovery.type", "single-node") // Single node mode for development
    .WithLifetime(ContainerLifetime.Persistent);


//var kibana = builder
//    .AddContainer("kibana", "elastic/kibana", "9.1.7") // Add Kibana from the image kibana, and tag 9.1.7 and give it a name kibana
//    .WithReference(elasticSearch) // Add a reference to the elasticsearch container
//    .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200") // Configure Elasticsearch connection
//    .WithEnvironment("ELASTICSEARCH_USERNAME", "elastic") // Elasticsearch username
//    .WithEnvironment("ELASTICSEARCH_PASSWORD", "changeme") // Elasticsearch password
//    .WithEnvironment("XPACK_SECURITY_ENABLED", "true") // MUST be enabled for Fleet
//    .WithEnvironment("XPACK_ENCRYPTEDSAVEDOBJECTS_ENCRYPTIONKEY", "a".PadRight(32, 'a')) // Required encryption key (32 chars min)
//    .WithEnvironment("XPACK_FLEET_AGENTS_ENABLED", "true") // Enable Fleet
//    .WithEnvironment("xpack.fleet.agents.fleet_server.hosts", "[\"http://fleet-server:8220\"]") // Fleet Server URL
//    .WithEnvironment("xpack.fleet.outputs", "[{\"id\":\"fleet-default-output\",\"name\":\"default\",\"type\":\"elasticsearch\",\"hosts\":[\"http://elasticsearch:9200\"],\"is_default\":true}]") // Fleet output config
//    .WithHttpEndpoint(5601, 5601)
//    .WithHttpHealthCheck("/status", 200, "http")
//    .WaitFor(elasticSearch)
//    .WithLifetime(ContainerLifetime.Persistent); // Expose a port so you can connect


//var apmServer = builder.AddContainer("apm-server", "elastic/apm-server", "9.1.7")
//   .WithReference(elasticSearch)
//   .WithEnvironment("output.elasticsearch.hosts", "[\"http://elasticsearch:9200\"]")
//   .WithEnvironment("output.elasticsearch.username", "elastic")
//   .WithEnvironment("output.elasticsearch.password", "changeme")
//   .WithEnvironment("apm-server.rum.enabled", "true")
//   .WithEnvironment("apm-server.auth.anonymous.enabled", "true") // Disable auth for development
//   .WithEnvironment("apm-server.kibana.enabled", "true")
//   .WithEnvironment("apm-server.kibana.host", "http://kibana:5601")
//   .WithEnvironment("apm-server.kibana.username", "elastic")
//   .WithEnvironment("apm-server.kibana.password", "changeme")
//   .WithHttpEndpoint(8200, 8200, "apm")
//   .WaitFor(elasticSearch)
//   .WaitFor(kibana)
//   .WithLifetime(ContainerLifetime.Persistent);

//// Fleet Server - required for Elastic Agent management
//var fleetServer = builder.AddContainer("fleet-server", "elastic/elastic-agent", "9.1.7")
//    .WithReference(elasticSearch)
//    .WithEnvironment("FLEET_SERVER_ENABLE", "1")
//    .WithEnvironment("FLEET_SERVER_ELASTICSEARCH_HOST", "http://elasticsearch:9200")
//    .WithEnvironment("FLEET_SERVER_ELASTICSEARCH_USERNAME", "elastic")
//    .WithEnvironment("FLEET_SERVER_ELASTICSEARCH_PASSWORD", "changeme")
//    .WithEnvironment("FLEET_SERVER_POLICY_ID", "fleet-server-policy")
//    .WithEnvironment("FLEET_SERVER_INSECURE_HTTP", "1") // Disable TLS for development
//    .WithEnvironment("FLEET_SERVER_ELASTICSEARCH_INSECURE", "1")
//    .WithEnvironment("KIBANA_FLEET_HOST", "http://kibana:5601")
//    .WithEnvironment("KIBANA_FLEET_USERNAME", "elastic")
//    .WithEnvironment("KIBANA_FLEET_PASSWORD", "changeme")
//    .WithHttpEndpoint(8220, 8220, "fleet")
//    .WaitFor(kibana)
//    .WithLifetime(ContainerLifetime.Persistent);

//var elasticAgent = builder.AddContainer("elastic-agent", "elastic/elastic-agent", "9.1.7")
//    .WithReference(elasticSearch)
//    .WithEnvironment("FLEET_ENROLL", "1")
//    .WithEnvironment("FLEET_URL", "http://fleet-server:8220")
//    .WithEnvironment("FLEET_ENROLLMENT_TOKEN", "enrollment-token-123") // This needs to be generated from Kibana Fleet UI
//    .WithEnvironment("FLEET_INSECURE", "1") // Disable TLS verification for development
//    .WithEnvironment("KIBANA_HOST", "http://kibana:5601")
//    .WithEnvironment("KIBANA_USERNAME", "elastic")
//    .WithEnvironment("KIBANA_PASSWORD", "changeme")
//    .WithEnvironment("ELASTICSEARCH_HOST", "http://elasticsearch:9200")
//    .WithEnvironment("ELASTICSEARCH_USERNAME", "elastic")
//    .WithEnvironment("ELASTICSEARCH_PASSWORD", "changeme")
//    .WaitFor(fleetServer)
//    .WaitFor(kibana)
//    .WithLifetime(ContainerLifetime.Persistent);

//var apiService = builder.AddProject<Projects.ElasticSearch_ApiService>("apiservice")
//    .WithHttpHealthCheck("/health")
//    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://apm-server:8200")
//    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc")
//    .WithEnvironment("OTEL_SERVICE_NAME", "ApiService")
//    .WaitFor(apmServer);

//builder.AddProject<Projects.ElasticSearch_Web>("webfrontend")
//    .WithExternalHttpEndpoints()
//    .WithHttpHealthCheck("/health")
//    .WithEnvironment("OTEL_SERVICE_NAME", "WebFrontEnd")
//    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://apm-server:8200")
//    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc")
//    .WithReference(apiService)
//    .WaitFor(apiService)
//    .WaitFor(apmServer);

builder.Build().Run();
