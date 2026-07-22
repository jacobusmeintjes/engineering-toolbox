using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseFramework.Core.Telemetry
{

    /// <summary>
    /// Wraps a channel execution delegate with tracing and metrics.
    /// Every client wrapper in Core/Clients should route its RestAssured.Net
    /// call through this decorator rather than emitting telemetry itself —
    /// keeps instrumentation in one place and makes the eventual extraction
    /// into a standalone TestFramework.Telemetry project a pure file move.
    /// </summary>
    public sealed class TelemetryChannelDecorator<TRequest, TResponse>
    {
        private readonly string _channelType;
        private readonly Func<TRequest, CancellationToken, Task<TResponse>>? _inner;
        private readonly Func<TRequest, TResponse>? _innerSync;
        public TelemetryChannelDecorator(
            string channelType,
            Func<TRequest, CancellationToken, Task<TResponse>> inner)
        {
            _channelType = channelType;
            _inner = inner;
        }

        public TelemetryChannelDecorator(string channelType, Func<TRequest, TResponse> inner)
        {
            _channelType = channelType;
            _innerSync = inner;
        }


        public TResponse Execute(TRequest request)
        {
            using var activity = TestFrameworkActivitySource.Source.StartActivity(
                name: "test.execute",
                kind: ActivityKind.Client);

            activity?.SetTag("test.channel_type", _channelType);
            activity?.SetTag("test.request_type", typeof(TRequest).Name);

            var correlationId = Guid.NewGuid().ToString("N");
            activity?.SetTag("test.correlation_id", correlationId);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = _innerSync(request);
                stopwatch.Stop();

                TestFrameworkMeters.TestsExecuted.Add(1,
                    new KeyValuePair<string, object?>("channel_type", _channelType),
                    new KeyValuePair<string, object?>("outcome", "success"));

                TestFrameworkMeters.TestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object?>("channel_type", _channelType));

                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                TestFrameworkMeters.TestsExecuted.Add(1,
                    new KeyValuePair<string, object?>("channel_type", _channelType),
                    new KeyValuePair<string, object?>("outcome", "failure"));

                TestFrameworkMeters.TestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object?>("channel_type", _channelType));

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                throw;
            }
        }


        public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ct = default)
        {
            using var activity = TestFrameworkActivitySource.Source.StartActivity(
                name: "test.execute",
                kind: ActivityKind.Client);

            activity?.SetTag("test.channel_type", _channelType);
            activity?.SetTag("test.request_type", typeof(TRequest).Name);

            var correlationId = Guid.NewGuid().ToString("N");
            activity?.SetTag("test.correlation_id", correlationId);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await _inner(request, ct);
                stopwatch.Stop();

                TestFrameworkMeters.TestsExecuted.Add(1,
                    new KeyValuePair<string, object?>("channel_type", _channelType),
                    new KeyValuePair<string, object?>("outcome", "success"));

                TestFrameworkMeters.TestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object?>("channel_type", _channelType));

                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                TestFrameworkMeters.TestsExecuted.Add(1,
                    new KeyValuePair<string, object?>("channel_type", _channelType),
                    new KeyValuePair<string, object?>("outcome", "failure"));

                TestFrameworkMeters.TestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object?>("channel_type", _channelType));

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                activity?.SetTag("exception.message", ex.Message);

                throw;
            }
        }
    }
}

