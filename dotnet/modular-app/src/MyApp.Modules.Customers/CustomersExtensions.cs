using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApp.Modules.Customers.Internal;
using MyApp.Modules.Customers.Public;

namespace MyApp.Modules.Customers
{
    public static class CustomersExtensions
    {
        public static IServiceCollection AddCustomersModule(this IServiceCollection services)
        {
            services.TryAddSingleton<ICustomersModule, CustomersModule>();

            services.TryAddSingleton<ICustomerService, CustomerService>();

            return services;
        }
    }
}
