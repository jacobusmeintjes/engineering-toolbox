using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace SolaceOboManager.Aspire
{
    public static class SolaceResourceReferenceExtensions
    {
        public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder,
            IResourceBuilder<SolaceResource> solaceResource)
            where TDestination : IResourceWithEnvironment
        {
            var resource = solaceResource.Resource;
            var resourceName = resource.Name;

            return builder.WithEnvironment(context =>
            {
                context.EnvironmentVariables["SOLACE_ADMIN_USERNAME"] = resource.AdminUsername;
                context.EnvironmentVariables["SOLACE_ADMIN_PASSWORD"] = resource.AdminPasswordParameter;
                context.EnvironmentVariables["SOLACE_VPNName"] = "default";

                foreach (var user in resource.Users)
                {
                    context.EnvironmentVariables[$"SOLACE_{user.Username.ToUpperInvariant()}_USERNAME"] = user.Username;
                    context.EnvironmentVariables[$"SOLACE_{user.Username.ToUpperInvariant()}_PASSWORD"] = user.Password;
                }


                // Main connection string uses the primary endpoint
                var httpEndpoint = solaceResource.GetEndpoint("host");
                context.EnvironmentVariables[$"SOLACE_HOST"] =
                    httpEndpoint.Property(EndpointProperty.Url);

                // Expose all endpoints
                var adminEndpoint = solaceResource.GetEndpoint("admin");
                context.EnvironmentVariables[$"SOLACE_ADMINURL"] =
                    adminEndpoint.Property(EndpointProperty.Url);

                var wsEndpoint = solaceResource.GetEndpoint("ws");
                context.EnvironmentVariables[$"SOLACE_WS"] =
                    wsEndpoint.Property(EndpointProperty.Url);

                var mqttEndpoint = solaceResource.GetEndpoint("mqtt");
                context.EnvironmentVariables[$"SOLACE_MQTT"] =
                    mqttEndpoint.Property(EndpointProperty.Url);

                var healthEndpoint = solaceResource.GetEndpoint("health");
                context.EnvironmentVariables[$"SOLACE_HEALTH"] =
                    healthEndpoint.Property(EndpointProperty.Url);



            });
        }
    }
}
