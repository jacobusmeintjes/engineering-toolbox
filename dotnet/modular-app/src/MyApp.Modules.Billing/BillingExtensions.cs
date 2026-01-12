using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Modules.Orders
{
    public static class BillingExtensions
    {
        public static IServiceCollection AddBillingModule(this IServiceCollection services)
        {


            return services;
        }
    }
}
