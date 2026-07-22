using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Sinks.OpenTelemetry;
using OpenTelemetry.Exporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EnterpriseTestFramework.LoadTests;

/// <summary>
/// NBomber load test for the User API, chosen (over k6) to stay natively in the .NET/C# stack.
/// Runs as a standalone console entry point on the nightly pipeline cadence — a "triggered"
/// fitness function rather than a continuous one.
///
/// Thresholds make this an actual pass/fail fitness function (p95/p99 latency budgets and
/// failure-rate ceiling) rather than an informational report: a regression returns a non-zero
/// exit code, which fails the nightly pipeline step.
/// </summary>
public static class UserApiLoadTests
{
    private const double P95BudgetMs = 500;
    private const double P99BudgetMs = 1000;

    public static int Run(string[] args)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("TARGET_BASE_URL") ?? "http://localhost:5000")
        };

        var scenario = Scenario.Create("get_user_by_id", async context =>
            {
                var httpMessage = Http.CreateRequest("GET", "/api/users/1")
                        .WithHeader("X-Correlation-Id", context.ScenarioInfo.InstanceId);
                var response = await Http.Send(httpClient, httpMessage);

                return response;
            })
            .WithLoadSimulations(
                Simulation.RampingInject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
                Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2))
            );

        // Real-time metrics stream into the same Prometheus/Grafana/Tempo stack as regular
        // application telemetry (ADR-0010) via NBomber's OpenTelemetry reporting sink —
        // either directly to an OTLP endpoint or through an OTel Collector.
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithTestSuite("EnterpriseTestFramework.LoadTests")
            .WithTestName("UserApi_SteadyState")
            .WithReportFolder("reports")
            .WithReportingSinks(
                new OpenTelemetrySink(new OtlpExporterOptions
                {
                    Endpoint = new Uri(otlpEndpoint),
                    Protocol = OtlpExportProtocol.Grpc
                }))
            .Run(args);

        // Post-run assertions on the aggregated stats — the documented NBomber pattern for
        // treating load results as pass/fail. Percentiles are less noise-sensitive than max.
        var stats = result.ScenarioStats.First(s => s.ScenarioName == "get_user_by_id");

        var failures = new List<string>();

        if (stats.Ok.Latency.Percent95 > P95BudgetMs)
        {
            failures.Add($"p95 latency {stats.Ok.Latency.Percent95}ms exceeded budget {P95BudgetMs}ms");
        }

        if (stats.Ok.Latency.Percent99 > P99BudgetMs)
        {
            failures.Add($"p99 latency {stats.Ok.Latency.Percent99}ms exceeded budget {P99BudgetMs}ms");
        }

        var totalRequests = stats.Ok.Request.Count + stats.Fail.Request.Count;
        var failRate = totalRequests == 0 ? 1.0 : (double)stats.Fail.Request.Count / totalRequests;
        if (failRate > 0.01)
        {
            failures.Add($"failure rate {failRate:P2} exceeded 1% ceiling");
        }

        if (failures.Count > 0)
        {
            Console.Error.WriteLine("Load test fitness function FAILED:");
            failures.ForEach(f => Console.Error.WriteLine($"  - {f}"));
            return 1;
        }

        Console.WriteLine($"Load test passed: p95={stats.Ok.Latency.Percent95}ms, p99={stats.Ok.Latency.Percent99}ms, failRate={failRate:P2}");
        return 0;
    }
}
