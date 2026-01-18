using keycloak.AppHost.Keycloak;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddKeycloak();

builder.Build().Run();

