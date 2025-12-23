using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Identity;

namespace SolaceOboManager.Aspire
{
    public static class SolaceResourceExtensions
    {
        public static IResourceBuilder<SolaceResource> WithSolaceVolume(this IResourceBuilder<SolaceResource> builder, string? name = null)
        {
            return builder.WithVolume(name ?? "storage-group", "/var/lib/solace");
        }
    }
}
