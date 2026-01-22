using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);
var postgresqlPassword = builder.AddParameter("postgres-password", "password", false);

// Postgres for Keycloak persistence
var postgres = builder.AddPostgres("keycloak-postgres", password: postgresqlPassword)
    .WithDataVolume() // Persist database data across restarts
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);



var keycloakDb = postgres.AddDatabase("keycloak");

//postgres.Resource.JdbcConnectionString

var keycloak = builder.AddContainer("keycloak-ui", "quay.io/keycloak/keycloak:26.0")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_DB", "postgres")
    // Construct JDBC URL manually
    .WithEnvironment("KC_DB_URL", $"jdbc:postgresql://{postgres.Resource.Name}:5432/keycloak")
    .WithEnvironment("KC_DB_USERNAME", "postgres")  // Default PostgreSQL username
    .WithEnvironment("KC_DB_PASSWORD", postgresqlPassword)
    .WithArgs("start-dev", "--http-port=8080")
    .WithHttpHealthCheck("/admin/master/console/", 200, "http")
    .WaitFor(postgres)
    .WithLifetime(ContainerLifetime.Persistent);

const string realmName = "demo";

// Simplified config - the provisioner now creates 4 tenants/clients and 16 users internally
var keycloakProvisioner = builder.AddProject<Projects.KeycloakProvisioningSample_KeycloakProvisioner>("keycloakprovisioner")
    .WithEnvironment("KEYCLOAK_URL", keycloak.GetEndpoint("http"))
    .WithEnvironment("KEYCLOAK_ADMIN_USERNAME", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KEYCLOAK_REALM", realmName)
    .WaitFor(keycloak);

var apiService = builder.AddProject<Projects.KeycloakProvisioningSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Keycloak__BaseUrl", keycloak.GetEndpoint("http"))
    .WithEnvironment("Keycloak__Realm", realmName)
    .WithEnvironment("Keycloak__Audience", "apiservice")
    .WaitForCompletion(keycloakProvisioner);

builder.AddProject<Projects.KeycloakProvisioningSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithEnvironment("Keycloak__BaseUrl", keycloak.GetEndpoint("http"))
    .WithEnvironment("Keycloak__Realm", realmName)
    .WithEnvironment("Keycloak__ClientId", "acme-corp")
    .WaitFor(apiService);

builder.Build().Run();
