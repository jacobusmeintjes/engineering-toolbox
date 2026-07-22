using System.Diagnostics.Metrics;

namespace EnterpriseFramework.Core.Telemetry
{
    /// <summary>
    /// Central Meter and metric instruments for the framework. Keeping these
    /// as static readonly fields (rather than creating instruments per-call)
    /// is required by the OTel Metrics API — instruments are meant to be
    /// long-lived and reused across every measurement.
    /// </summary>
    public static class TestFrameworkMeters
    {
        public const string Name = "EnterpriseFramework.Core";
        public const string Version = "1.0.0";

        private static readonly Meter Meter = new(Name, Version);

        public static readonly Counter<long> TestsExecuted = Meter.CreateCounter<long>(
            name: "tests.executed",
            unit: "{test}",
            description: "Number of test executions completed, tagged by outcome and channel_type.");

        public static readonly Histogram<double> TestDuration = Meter.CreateHistogram<double>(
            name: "tests.duration.ms",
            unit: "ms",
            description: "Wall-clock duration of a single test channel execution.");

        public static readonly Counter<long> AssertionFailures = Meter.CreateCounter<long>(
            name: "tests.assertions.failed",
            unit: "{assertion}",
            description: "Number of individual assertion failures, tagged by assertion type.");

        public static readonly Counter<long> RetriesAttempted = Meter.CreateCounter<long>(
            name: "tests.retries",
            unit: "{retry}",
            description: "Number of retry attempts made during a test execution (useful for flakiness tracking).");
    }
}
