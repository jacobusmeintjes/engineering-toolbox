using Fluxor;
using SolaceOboManager.Benchmarks.Solace;

namespace BlazorRedux.Web.Workers
{
    public class Worker : BackgroundService
    {
        private readonly SessionFactory _sessionFactory;
        private readonly IConfiguration _configuration;

        public Worker(SessionFactory sessionFactory, IServiceProvider sp, IConfiguration configuration)
        {
            var scope = sp.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
            _sessionFactory = sessionFactory;
            _configuration = configuration;
            _sessionFactory.SessionEvents.Subscribe(c =>
            {                
                dispatcher.Dispatch(new BlazorRedux.Web.Features.Action.IncrementCounterAction());
            });
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            var host = _configuration.GetValue<string>("SOLACE_HOST");
            var vpnName = _configuration.GetValue<string>("SOLACE_VpnName");
            var username = _configuration.GetValue<string>("SOLACE_PUBLISHER_USERNAME");
            var password = _configuration.GetValue<string>("SOLACE_PUBLISHER_PASSWORD");

            await Task.Delay(20_000);

            var session = _sessionFactory.CreateSession(host, vpnName, username, password);
        }
    }
}
