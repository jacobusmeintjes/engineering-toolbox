using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseFramework.Core.Chaos
{
    /// <summary>
    /// Simmy-style chaos implementation. Configuration is reloadable at runtime via
    /// IOptionsMonitor, so scenarios can be toggled per-test-run without redeploying.
    /// Uses Random.Shared for thread safety under parallel test execution, and structured
    /// logging with a [Chaos] prefix so injected failures are distinguishable from real ones.
    /// </summary>
    public sealed class PollySimmyChaosEngine : IChaosEngine
    {
        private readonly IOptionsMonitor<ChaosOptions> _options;
        private readonly ILogger<PollySimmyChaosEngine> _logger;
        private readonly Dictionary<string, int> _callCounters = new();
        private readonly object _counterLock = new();

        public PollySimmyChaosEngine(IOptionsMonitor<ChaosOptions> options, ILogger<PollySimmyChaosEngine> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<T> ExecuteAsync<T>(string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            var options = _options.CurrentValue;
            if (!options.Enabled || !options.Scenarios.TryGetValue(operationName, out var scenario))
            {
                return await operation();
            }

            if (Random.Shared.NextDouble() > scenario.InjectionProbability)
            {
                return await operation();
            }

            switch (scenario.Mode)
            {
                case ChaosFailureMode.ThrowException:
                    _logger.LogWarning("[Chaos] Injecting immediate failure for {OperationName} ({ExceptionType})",
                        operationName, scenario.ExceptionType);
                    throw CreateException(scenario);

                case ChaosFailureMode.SlowThenThrow:
                    _logger.LogWarning("[Chaos] Injecting latency ({LatencyBase} + jitter) then failure for {OperationName}",
                        scenario.LatencyBase, operationName);
                    await InjectLatencyAsync(scenario, cancellationToken);
                    throw CreateException(scenario);

                case ChaosFailureMode.IntermittentFlap:
                    int count;
                    lock (_counterLock)
                    {
                        _callCounters.TryGetValue(operationName, out count);
                        count++;
                        _callCounters[operationName] = count;
                    }
                    if (count % Math.Max(1, scenario.FlapEveryNCalls) == 0)
                    {
                        _logger.LogWarning("[Chaos] Flap failure for {OperationName} on call #{Count} (every {N} calls)",
                            operationName, count, scenario.FlapEveryNCalls);
                        throw CreateException(scenario);
                    }
                    return await operation();

                default:
                    return await operation();
            }
        }

        private static async Task InjectLatencyAsync(ChaosScenario scenario, CancellationToken cancellationToken)
        {
            var jitterMs = scenario.LatencyJitter.TotalMilliseconds > 0
                ? Random.Shared.NextDouble() * scenario.LatencyJitter.TotalMilliseconds
                : 0;
            var delay = scenario.LatencyBase + TimeSpan.FromMilliseconds(jitterMs);
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        private static Exception CreateException(ChaosScenario scenario)
        {
            var type = Type.GetType(scenario.ExceptionType) ?? typeof(TimeoutException);
            return (Exception)(Activator.CreateInstance(type, $"[Chaos] injected failure via {type.Name}") ?? new TimeoutException("[Chaos] injected failure"));
        }
    }
}
