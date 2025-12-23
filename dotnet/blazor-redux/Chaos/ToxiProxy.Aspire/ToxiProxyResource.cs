using Aspire.Hosting.ApplicationModel;

namespace ToxiProxy.Aspire
{
    public class ToxiProxyResource(string name) : ContainerResource(name), IResourceWithEndpoints, IResourceWithEnvironment
    {
        public const string DefaultImage = "ghcr.io/shopify/toxiproxy";
        public const string DefaultTag = "latest";
    }
}
