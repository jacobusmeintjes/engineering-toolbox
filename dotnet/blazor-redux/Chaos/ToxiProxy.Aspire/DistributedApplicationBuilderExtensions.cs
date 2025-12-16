using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace ToxiProxy.Aspire
{
    public static class DistributedApplicationBuilderExtensions
    {
        public static IResourceBuilder<ToxiProxyResource> AddToxiProxy(this IDistributedApplicationBuilder? builder,
            string name,
            int? httpPort = 8474)
        {            
            var toxiProxyResource = new ToxiProxyResource(name);

            // Add Toxiproxy container
            var resourceBuilder = builder?.AddResource(toxiProxyResource)
                .WithImage(ToxiProxyResource.DefaultImage)                
                .WithImageTag(ToxiProxyResource.DefaultTag);

            // Add HTTP endpoint - this automatically implements IResourceWithEndpoints
            resourceBuilder.WithHttpEndpoint(
                port: httpPort,           // External port (optional, auto-assigned if null)
                targetPort: 8474,         // Container internal port
                name: "http",
                isProxied: false);            // Endpoint name for reference

            return resourceBuilder;
        }
    }
}
