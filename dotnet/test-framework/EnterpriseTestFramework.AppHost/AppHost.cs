var builder = DistributedApplication.CreateBuilder(args);

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("../prometheus", "/etc/prometheus")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml")
    .WithHttpEndpoint(port: 9090, targetPort: 9090);

var tempo = builder.AddContainer("tempo", "grafana/tempo", "2.6.1")
    .WithBindMount("../tempo", "/etc/tempo")
    .WithArgs("-config.file=/etc/tempo/tempo.yml")
    .WithHttpEndpoint(port: 3200, targetPort: 3200)
    .WithEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc");

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithBindMount("../grafana/dashboards", "/etc/grafana/provisioning/dashboards")
    .WithBindMount("../grafana/datasources", "/etc/grafana/provisioning/datasources")
    .WithHttpEndpoint(port: 3000, targetPort: 3000)
    .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true"); // fine for local/dev



builder.Build().Run();
