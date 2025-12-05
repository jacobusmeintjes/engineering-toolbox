using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace SolaceOboManager.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        }
    }
}


