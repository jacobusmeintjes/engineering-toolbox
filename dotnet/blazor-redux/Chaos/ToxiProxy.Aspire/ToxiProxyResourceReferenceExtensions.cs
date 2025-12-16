using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace ToxiProxy.Aspire
{
    public static class ToxiProxyResourceReferenceExtensions
    {
        public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder,
            IResourceBuilder<ToxiProxyResource> source) where TDestination : IResourceWithEnvironment
        {
            var resource = source.Resource;
            var name = resource.Name;

            return builder.WithEnvironment(context =>
            {
                 // Alternative: Use GetEndpoint helper
                var endpoint = resource.GetEndpoint("http");
                context.EnvironmentVariables[$"{name}_BaseUrl".ToUpperInvariant()] =
                    endpoint.Property(EndpointProperty.Url);
            });
        }
    }
}
