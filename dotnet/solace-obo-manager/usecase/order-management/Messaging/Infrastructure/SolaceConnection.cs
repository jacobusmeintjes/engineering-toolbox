using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;

namespace Messaging.Infrastructure
{
    // Infrastructure/SolaceConnection.cs
    public sealed class SolaceConnection : IDisposable
    {
        private readonly SolaceOptions _options;
        private readonly ILogger<SolaceConnection> _logger;
        private IContext? _context;
        private ISession? _session;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _disposed;

        public SolaceConnection(
            IOptions<SolaceOptions> options,
            ILogger<SolaceConnection> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public ISession Session =>
            _session ?? throw new InvalidOperationException(
                "Solace session not initialised — ensure SolaceConnectionInitialiser has run");

        public async Task InitialiseAsync(CancellationToken ct)
        {
            await _lock.WaitAsync(ct);
            try
            {
                if (_session is not null) return;

                _logger.LogInformation(
                    "Initialising Solace connection to {Host} VPN {VpnName}",
                    _options.Host, _options.VpnName);

                ContextFactoryProperties cfp = new()
                {
                    SolClientLogLevel = SolLogLevel.Warning
                };

                ContextFactory.Instance.Init(cfp);

                _context = ContextFactory.Instance.CreateContext(
                    new ContextProperties(), null);

                SessionProperties sp = new()
                {
                    Host = _options.Host,
                    VPNName = _options.VpnName,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    ReconnectRetries = _options.ReconnectRetries,
                    ReconnectRetriesWaitInMsecs = _options.ReconnectRetriesWaitInMsecs,
                    ConnectTimeoutInMsecs = 10000
                };

                _session = _context.CreateSession(sp, null, OnSessionEvent);

                var result = _session.Connect();

                if (result != ReturnCode.SOLCLIENT_OK)
                    throw new InvalidOperationException(
                        $"Failed to connect to Solace: {result}");

                _logger.LogInformation(
                    "Connected to Solace at {Host}", _options.Host);
            }
            finally
            {
                _lock.Release();
            }
        }

        private void OnSessionEvent(object? sender, SessionEventArgs e)
        {
            switch (e.Event)
            {
                case SessionEvent.Reconnecting:
                    _logger.LogWarning(
                        "Solace session reconnecting — info: {Info}", e.Info);
                    break;

                case SessionEvent.Reconnected:
                    _logger.LogInformation("Solace session reconnected");
                    break;

                case SessionEvent.DownError:
                    _logger.LogError(
                        "Solace session down — info: {Info}", e.Info);
                    break;

                case SessionEvent.ConnectFailedError:
                    _logger.LogError(
                        "Solace connection failed — info: {Info}", e.Info);
                    break;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogInformation("Disposing Solace connection");

            _session?.Disconnect();
            _session?.Dispose();
            _context?.Dispose();
            _lock.Dispose();

            ContextFactory.Instance.Cleanup();
        }
    }
}
