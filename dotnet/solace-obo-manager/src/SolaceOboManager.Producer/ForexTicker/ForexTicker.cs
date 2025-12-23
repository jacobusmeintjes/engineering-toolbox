namespace SolaceOboManager.Producer
{
    public record ForexTick(string Token, string Pair, decimal Bid, decimal Ask, DateTime Timestamp);

    public class ForexTicker
    {
        private readonly Random _random = new();
        private readonly Dictionary<string, (decimal basePrice, decimal spread)> _pairs;
        
        public event Action<ForexTick>? OnTick;

        public ForexTicker()
        {
            _pairs = new Dictionary<string, (decimal basePrice, decimal spread)>
            {
                ["EUR/USD"] = (1.1000m, 0.0002m),
                ["GBP/USD"] = (1.2500m, 0.0003m),
                ["USD/JPY"] = (140.00m, 0.05m),
                ["AUD/USD"] = (0.6600m, 0.0002m),
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var list = new List<Task>();
                foreach (var pair in _pairs)
                {
                    var tick = GenerateTick(pair.Key, pair.Value.basePrice, pair.Value.spread);
                    OnTick?.Invoke(tick);
                }

//                await Task.Delay(1000, cancellationToken);
            }
        }

        private ForexTick GenerateTick(string pair, decimal basePrice, decimal spread)
        {
            var mid = basePrice + ((decimal)_random.NextDouble() - 0.5m) * 0.01m;
            var bid = mid - spread / 2;
            var ask = mid + spread / 2;


            return new ForexTick(Guid.NewGuid().ToString(), pair, Math.Round(bid, 5), Math.Round(ask, 5), DateTime.UtcNow);
        }
    }
}
