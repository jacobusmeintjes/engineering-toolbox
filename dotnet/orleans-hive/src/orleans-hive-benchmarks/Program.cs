

// Must run in Release mode: dotnet run -c Release
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using orleans_hive_benchmarks;

BenchmarkRunner.Run<CounterGrainBenchmarks>();