namespace ChaosController.Orchestrators
{
    public class ChaosOrchestrator
    {
//        private readonly IToxiproxyClient _client;
//        private readonly ILogger<ChaosOrchestrator> _logger;
//        private readonly Dictionary<string, List<string>> _activeToxics = new();

//        public ChaosOrchestrator(IToxiproxyClient client, ILogger<ChaosOrchestrator> logger)
//        {
//            _client = client;
//            _logger = logger;
//        }

//        public async Task BreakRedisConnectionAsync()
//        {
//            var proxy = await _client.GetProxyAsync("redis_proxy");

//            var toxic = await proxy.Toxics().TimeoutAsync(
//                "connection_timeout",
//                ToxicDirection.Downstream,
//                timeout: 0
//            );

//            TrackToxic("redis_proxy", toxic.Name);
//            _logger.LogWarning("Redis connection broken");
//        }

//        public async Task AddLatencyAsync(string proxyName, int milliseconds)
//        {
//            var proxy = await _client.GetProxyAsync(proxyName);

//            var toxic = await proxy.Toxics().LatencyAsync(
//                "added_latency",
//                ToxicDirection.Downstream,
//                latency: milliseconds
//            );

//            TrackToxic(proxyName, toxic.Name);
//            _logger.LogWarning("Added {Latency}ms latency to {Proxy}", milliseconds, proxyName);
//        }

//        public async Task AddPacketLossAsync(string proxyName, float percentage)
//        {
//            var proxy = await _client.GetProxyAsync(proxyName);

//            var toxic = await proxy.Toxics().SlicerAsync(
//                "packet_loss",
//                ToxicDirection.Downstream,
//                averageSize: 1000,
//                sizeVariation: 500,
//                delay: 10
//            );

//            TrackToxic(proxyName, toxic.Name);
//        }

//        public async Task RestoreAllAsync()
//        {
//            var proxies = await _client.GetAllProxiesAsync();

//            foreach (var proxy in proxies)
//            {
//                var toxics = await proxy.Toxics().GetAllAsync();
//                foreach (var toxic in toxics)
//                {
//                    await toxic.RemoveAsync();
//                }
//            }

//            _activeToxics.Clear();
//            _logger.LogInformation("All toxics removed");
//        }

//        private void TrackToxic(string proxyName, string toxicName)
//        {
//            if (!_activeToxics.ContainsKey(proxyName))
//                _activeToxics[proxyName] = new();

//            _activeToxics[proxyName].Add(toxicName);
//        }
    }
}
