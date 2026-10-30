var builder = DistributedApplication.CreateBuilder(args);

// Spin up OpenSearch as a generic container
//var opensearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "2.11.0")
//    .WithHttpEndpoint(port: 9200, targetPort: 9200, name: "http")
//    .WithEnvironment("discovery.type", "single-node")
//    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
//    .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
//    .WithVolume("opensearch-data", "/usr/share/opensearch/data")
//    .WithHttpHealthCheck("/_cluster/health", endpointName: "http");

//builder.AddContainer("opensearch-dashboards", "opensearchproject/opensearch-dashboards", "2.11.0")
//    .WithHttpEndpoint(port: 5601, targetPort: 5601, name: "http")
//    .WithEnvironment("OPENSEARCH_HOSTS", "[\"http://opensearch:9200\"]")
//    .WithEnvironment("DISABLE_SECURITY_DASHBOARDS_PLUGIN", "true")
//    .WaitFor(opensearch);

//var dataPrepper = builder.AddContainer("data-prepper", "opensearchproject/data-prepper", "2")
//    .WithBindMount(Path.Combine(builder.AppHostDirectory, "pipelines.yaml"),
//                   "/usr/share/data-prepper/pipelines/pipelines.yaml")
//    .WithEndpoint(targetPort: 21890, name: "traces", scheme: "http")
//    .WithEndpoint(targetPort: 21891, name: "metrics", scheme: "http")
//    .WithEndpoint(targetPort: 21892, name: "logs", scheme: "http")
//    .WaitFor(opensearch);


// ---------------------------------------------------------------------------
// SigNoz: ZooKeeper + ClickHouse + schema migrators + SigNoz (UI/API) + SigNoz OTel collector
// Config files live in ./signoz. UI at http://localhost:8080
// ---------------------------------------------------------------------------
const string clickHouseTag = "25.5.6";
const string signozTag = "v0.96.1";
const string signozCollectorTag = "v0.129.6";
var signozDir = Path.Combine(builder.AppHostDirectory, "signoz");

var zookeeper = builder.AddContainer("zookeeper-1", "signoz/zookeeper", "3.7.1")
    .WithEnvironment("ZOO_SERVER_ID", "1")
    .WithEnvironment("ALLOW_ANONYMOUS_LOGIN", "yes")
    .WithEnvironment("ZOO_AUTOPURGE_INTERVAL", "1")
    .WithVolume("signoz-zookeeper", "/bitnami/zookeeper");

// One-shot job that downloads the histogramQuantile UDF binary used by ClickHouse
var initClickhouse = builder.AddContainer("signoz-init-clickhouse", "clickhouse/clickhouse-server", clickHouseTag)
    .WithEntrypoint("/bin/bash")
    .WithArgs("-c",
        "set -e; cd /tmp; " +
        "os=$(uname -s | tr '[:upper:]' '[:lower:]'); " +
        "arch=$(uname -m | sed 's/aarch64/arm64/;s/x86_64/amd64/'); " +
        "wget -O hq.tar.gz \"https://github.com/SigNoz/signoz/releases/download/histogram-quantile%2Fv0.0.1/histogram-quantile_${os}_${arch}.tar.gz\"; " +
        "tar -xzf hq.tar.gz; " +
        "mkdir -p /var/lib/clickhouse/user_scripts; " +
        "mv histogram-quantile /var/lib/clickhouse/user_scripts/histogramQuantile; " +
        "chmod +x /var/lib/clickhouse/user_scripts/histogramQuantile")
    .WithVolume("signoz-clickhouse-user-scripts", "/var/lib/clickhouse/user_scripts");

var clickhouse = builder.AddContainer("clickhouse", "clickhouse/clickhouse-server", clickHouseTag)
    .WithHttpEndpoint(targetPort: 8123, name: "http")
    .WithEndpoint(targetPort: 9000, name: "tcp", scheme: "tcp")
    .WithBindMount(Path.Combine(signozDir, "clickhouse", "cluster.xml"), "/etc/clickhouse-server/config.d/cluster.xml", isReadOnly: true)
    .WithBindMount(Path.Combine(signozDir, "clickhouse", "custom_function.xml"), "/etc/clickhouse-server/custom_function.xml", isReadOnly: true)
    .WithVolume("signoz-clickhouse", "/var/lib/clickhouse")
    .WithVolume("signoz-clickhouse-user-scripts", "/var/lib/clickhouse/user_scripts")
    .WithEnvironment("CLICKHOUSE_SKIP_USER_SETUP", "1")
    .WithHttpHealthCheck("/ping", endpointName: "http")
    .WaitFor(zookeeper)
    .WaitForCompletion(initClickhouse);

var migratorSync = builder.AddContainer("signoz-schema-migrator-sync", "signoz/signoz-schema-migrator", signozCollectorTag)
    .WithArgs("sync", "--dsn=tcp://clickhouse:9000", "--up=")
    .WithContainerRuntimeArgs("--restart=on-failure:10")
    .WaitFor(clickhouse);

builder.AddContainer("signoz-schema-migrator-async", "signoz/signoz-schema-migrator", signozCollectorTag)
    .WithArgs("async", "--dsn=tcp://clickhouse:9000", "--up=")
    .WithContainerRuntimeArgs("--restart=on-failure:10")
    .WaitFor(clickhouse)
    .WaitForCompletion(migratorSync);

var signoz = builder.AddContainer("signoz", "signoz/signoz", signozTag)
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("SIGNOZ_ALERTMANAGER_PROVIDER", "signoz")
    .WithEnvironment("SIGNOZ_TELEMETRYSTORE_CLICKHOUSE_DSN", "tcp://clickhouse:9000")
    .WithEnvironment("SIGNOZ_SQLSTORE_SQLITE_PATH", "/var/lib/signoz/signoz.db")
    .WithEnvironment("SIGNOZ_TOKENIZER_JWT_SECRET", "secret")
    .WithEnvironment("DOT_METRICS_ENABLED", "true")
    .WithVolume("signoz-sqlite", "/var/lib/signoz")
    .WithHttpHealthCheck("/api/v1/health", endpointName: "http")
    .WaitFor(clickhouse)
    .WaitForCompletion(migratorSync);

var signozCollector = builder.AddContainer("signoz-otel-collector", "signoz/signoz-otel-collector", signozCollectorTag)
    .WithEntrypoint("/signoz-otel-collector")
    .WithArgs(
        "--config=/etc/otel-collector-config.yaml",
        "--manager-config=/etc/manager-config.yaml",
        "--copy-path=/var/tmp/collector-config.yaml")
    .WithBindMount(Path.Combine(signozDir, "otel-collector-config.yaml"), "/etc/otel-collector-config.yaml", isReadOnly: true)
    .WithBindMount(Path.Combine(signozDir, "otel-collector-manager-config.yaml"), "/etc/manager-config.yaml", isReadOnly: true)
    .WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "host.name=signoz-host,os.type=linux")
    .WithEnvironment("LOW_CARDINAL_EXCEPTION_GROUPING", "false")
    .WithEndpoint(targetPort: 4317, name: "otlp-grpc", scheme: "http")
    .WithEndpoint(targetPort: 4318, name: "otlp-http", scheme: "http")
    .WaitFor(signoz)
    .WaitForCompletion(migratorSync);

// collector must wait for it

var collector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "latest")
       .WithBindMount(Path.Combine(builder.AppHostDirectory, "otel-config.yaml"), "/etc/otelcol-contrib/config.yaml")
    .WithEndpoint(targetPort: 4317, name: "otlp-grpc", scheme: "http")
   // .WaitFor(dataPrepper)
    .WaitFor(signozCollector);

// // Reference the container in your MCP Server project
// builder.AddProject<Projects.OpenSearchMcpServer>("mcp-server")
//        .WithReference(openSearch);

builder.AddProject<Projects.TestMetrics>("test-metrics")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", collector.GetEndpoint("otlp-grpc"))
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc");;


builder.Build().Run();
