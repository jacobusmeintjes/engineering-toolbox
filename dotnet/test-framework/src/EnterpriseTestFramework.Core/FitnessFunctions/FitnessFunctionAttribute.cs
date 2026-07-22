namespace EnterpriseFramework.Core.FitnessFunctions
{
    public enum FitnessCategory
    {
        Structural,
        Performance,
        Resiliency,
        Security,
        Observability
    }

    public enum FitnessCadence
    {
        /// <summary>Runs on every PR — cheap and merge-blocking.</summary>
        Continuous,
        /// <summary>Runs on a schedule (nightly/pre-release) — expensive, informative.</summary>
        Triggered
    }

    /// <summary>
    /// Tags a test as an architecture fitness function so the full catalog of fitness functions,
    /// their cadence, and their owners can be discovered mechanically rather than tribally.
    /// Applied to ArchUnitNET rules, latency-budget assertions, chaos/resiliency tests, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class FitnessFunctionAttribute : Attribute
    {
        public FitnessFunctionAttribute(FitnessCategory category, FitnessCadence cadence, string owner, string rationale)
        {
            Category = category;
            Cadence = cadence;
            Owner = owner;
            Rationale = rationale;
        }

        public FitnessCategory Category { get; }
        public FitnessCadence Cadence { get; }
        public string Owner { get; }
        public string Rationale { get; }
    }
}
