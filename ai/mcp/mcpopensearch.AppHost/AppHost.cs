var builder = DistributedApplication.CreateBuilder(args);

// Spin up OpenSearch as a generic container
var opensearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "2.11.0")
    .WithHttpEndpoint(port: 9200, targetPort: 9200, name: "http")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
    .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithVolume("opensearch-data", "/usr/share/opensearch/data")
    .WithHttpHealthCheck("/_cluster/health", endpointName: "http");

builder.AddContainer("opensearch-dashboards", "opensearchproject/opensearch-dashboards", "2.11.0")
    .WithHttpEndpoint(port: 5601, targetPort: 5601, name: "http")
    .WithEnvironment("OPENSEARCH_HOSTS", "[\"http://opensearch:9200\"]")
    .WithEnvironment("DISABLE_SECURITY_DASHBOARDS_PLUGIN", "true")
    .WaitFor(opensearch);

var dataPrepper = builder.AddContainer("data-prepper", "opensearchproject/data-prepper", "2")
    .WithBindMount(Path.Combine(builder.AppHostDirectory, "pipelines.yaml"),
                   "/usr/share/data-prepper/pipelines/pipelines.yaml")
    .WithEndpoint(targetPort: 21890, name: "traces", scheme: "http")
    .WithEndpoint(targetPort: 21891, name: "metrics", scheme: "http")
    .WithEndpoint(targetPort: 21892, name: "logs", scheme: "http")
    .WaitFor(opensearch);

// collector must wait for it

var collector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "latest")
       .WithBindMount(Path.Combine(builder.AppHostDirectory, "otel-config.yaml"), "/etc/otelcol-contrib/config.yaml")
    .WithEndpoint(targetPort: 4317, name: "otlp-grpc", scheme: "http")
    .WaitFor(dataPrepper);

// // Reference the container in your MCP Server project
// builder.AddProject<Projects.OpenSearchMcpServer>("mcp-server")
//        .WithReference(openSearch);

builder.AddProject<Projects.TestMetrics>("test-metrics")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", collector.GetEndpoint("otlp-grpc"))
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc");;


builder.Build().Run();
