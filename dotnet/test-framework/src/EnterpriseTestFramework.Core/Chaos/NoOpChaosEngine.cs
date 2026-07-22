namespace EnterpriseFramework.Core.Chaos
{
    /// <summary>
    /// Production-safety default. Registered whenever ChaosOptions.Enabled is false or the
    /// environment is not explicitly whitelisted for chaos — a two-key safeguard so chaos can
    /// never activate by accident outside controlled test runs.
    /// </summary>
    public sealed class NoOpChaosEngine : IChaosEngine
    {
        public Task<T> ExecuteAsync<T>(string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default) =>
            operation();
    }
}
