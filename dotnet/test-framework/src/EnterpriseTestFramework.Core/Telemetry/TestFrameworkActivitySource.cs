using System.Diagnostics;

namespace EnterpriseFramework.Core.Telemetry
{
    /// <summary>
    /// Central ActivitySource for the framework. All test execution spans
    /// are created from this single instance so they group correctly in
    /// Tempo/Jaeger under one service/source name.
    /// </summary>
    public static class TestFrameworkActivitySource
    {
        public const string Name = "EnterpriseFramework.Core";
        public const string Version = "1.0.0";

        public static readonly ActivitySource Source = new(Name, Version);
    }

}