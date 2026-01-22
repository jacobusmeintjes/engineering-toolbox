var builder = DistributedApplication.CreateBuilder(args);

var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);

// Postgres for Keycloak persistence
var keycloakDb = builder.AddPostgres("keycloak-postgres")
    .WithDataVolume() // Persist database data across restarts
    .AddDatabase("keycloak");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.0")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", keycloakDb)
    .WithEnvironment("KC_DB_USERNAME", keycloakDb)
    .WithEnvironment("KC_DB_PASSWORD", keycloakDb)
    .WithArgs("start-dev", "--http-port=8080")
    .WaitFor(keycloakDb);

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
    .WaitFor(keycloakProvisioner);

builder.AddProject<Projects.KeycloakProvisioningSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithEnvironment("Keycloak__BaseUrl", keycloak.GetEndpoint("http"))
    .WithEnvironment("Keycloak__Realm", realmName)
    .WithEnvironment("Keycloak__ClientId", "acme-corp")
    .WaitFor(apiService);

builder.Build().Run();
