using Messaging.Abstractions;
using Messaging.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging
{
    // MessagingServiceExtensions.cs
    public static class MessagingServiceExtensions
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SolaceOptions>(
                configuration.GetSection(SolaceOptions.SectionName));

            services.AddSingleton<SolaceConnection>();
            services.AddSingleton<IEventPublisher, SolacePublisher>();

            // Initialises the connection before the app starts serving requests
            services.AddHostedService<SolaceConnectionInitialiser>();

            services.AddHealthChecks()
                .AddCheck<SolaceHealthCheck>("solace");

            return services;
        }

        // Called per subscriber — e.g. services.AddSubscriber<OrderPlaced, OrderPlacedHandler>()
        public static IServiceCollection AddSubscriber<TEvent, THandler>(
            this IServiceCollection services)
            where TEvent : class, IEvent
            where THandler : class, IEventHandler<TEvent>
        {
            services.AddScoped<IEventHandler<TEvent>, THandler>();
            return services;
        }
    }
}
