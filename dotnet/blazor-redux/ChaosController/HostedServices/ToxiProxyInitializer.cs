//namespace ChaosController.HostedServices
//{
//    public class ToxiproxyInitializer : IHostedService
//    {
//        private readonly IToxiproxyClient _client;
//        private readonly ILogger<ToxiproxyInitializer> _logger;

//        public ToxiproxyInitializer(IToxiproxyClient client, ILogger<ToxiproxyInitializer> logger)
//        {
//            _client = client;
//            _logger = logger;
//        }

//        public async Task StartAsync(CancellationToken cancellationToken)
//        {
//            // Wait for Toxiproxy to be ready
//            await Task.Delay(2000, cancellationToken);

//            try
//            {
//                // Create proxies
//                await _client.AddAsync(new Proxy
//                {
//                    Name = "redis_proxy",
//                    Enabled = true,
//                    Listen = "0.0.0.0:6380",
//                    Upstream = "redis:6379" // Aspire service name
//                });

//                await _client.AddAsync(new Proxy
//                {
//                    Name = "postgres_proxy",
//                    Enabled = true,
//                    Listen = "0.0.0.0:5433",
//                    Upstream = "postgres:5432"
//                });

//                _logger.LogInformation("Toxiproxy proxies initialized");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to initialize Toxiproxy");
//            }
//        }

//        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
//    }
//}
