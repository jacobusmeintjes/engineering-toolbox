using System.Diagnostics;

namespace Messaging.Infrastructure
{
    // Infrastructure/MessagingTelemetry.cs
    public static class MessagingTelemetry
    {
        public const string SourceName = "Messaging";
        public const string Version = "1.0.0";

        public static readonly ActivitySource Source =
            new(SourceName, Version);

        // W3C TraceContext header names — what we inject/extract from Solace user properties
        public const string TraceParentHeader = "traceparent";
        public const string TraceStateHeader = "tracestate";
    }
}
