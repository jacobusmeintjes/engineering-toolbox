using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApp.Modules.Orders.Internal;
using MyApp.Modules.Orders.Public;

namespace MyApp.Modules.Orders
{
    public static class OrdersExtensions
    {
        public static IServiceCollection AddOrdersModule(this IServiceCollection services)
        {
            services.TryAddSingleton<IOrderService, OrderService>();

            services.TryAddSingleton<IOrdersModule, OrdersModule>();

            return services;
        }
    }
}
