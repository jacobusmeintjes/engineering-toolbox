namespace EnterpriseFramework.Core.Chaos
{
    /// <summary>
    /// Chaos injection contract, registered via DI and gated by a feature flag/environment check.
    /// Business/test code depends only on this interface — chaos logic never appears inline in
    /// test bodies or production code, and is fully removable via NoOpChaosEngine.
    /// </summary>
    public interface IChaosEngine
    {
        Task<T> ExecuteAsync<T>(string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default);
    }

    public enum ChaosFailureMode
    {
        ThrowException,
        SlowThenThrow,
        IntermittentFlap
    }

    public sealed class ChaosOptions
    {
        public bool Enabled { get; set; }
        public Dictionary<string, ChaosScenario> Scenarios { get; set; } = new();
    }

    public sealed class ChaosScenario
    {
        public ChaosFailureMode Mode { get; set; }
        public double InjectionProbability { get; set; }
        public TimeSpan LatencyBase { get; set; } = TimeSpan.Zero;
        public TimeSpan LatencyJitter { get; set; } = TimeSpan.Zero;
        public string ExceptionType { get; set; } = "System.TimeoutException";
        public int FlapEveryNCalls { get; set; } = 3;
    }
}
