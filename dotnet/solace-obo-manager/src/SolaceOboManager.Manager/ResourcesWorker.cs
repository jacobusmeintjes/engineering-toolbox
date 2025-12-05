using SolaceOboManager.Shared;
using SolaceSystems.Solclient.Messaging;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Solace.Manager.UI.HostedServices
{
    public sealed record CreateResource(string ClientName, Guid Id);

    public sealed record ResourceCreated(Guid Id, string Queue, string SubscriptionTopic, string HealthCheckTopic);

    public class ResourcesWorker : BackgroundService
    {
        private const string TopicName = "resources/registration";
        private readonly SolaceConfiguration _solaceConfiguration;
        private readonly ILogger<ResourcesWorker> _logger;
        private readonly ChannelWriter<CreateResource> _channelWriter;
        private ISession _session;

        public ResourcesWorker(SolaceConfiguration solaceConfiguration,
            ILogger<ResourcesWorker> logger,
            ChannelWriter<CreateResource> channelWriter)
        {
            _solaceConfiguration = solaceConfiguration;
            _logger = logger;
            _channelWriter = channelWriter;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Initialize Solace Systems Messaging API with logging to console at Warning level
            var cfp = new ContextFactoryProperties()
            {
                SolClientLogLevel = SolLogLevel.Warning
            };
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);

            IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null);

            // Create session properties
            SessionProperties sessionProps = new SessionProperties()
            {
                Host = "tcp://localhost:15555",
                VPNName = _solaceConfiguration.VPNName,
                UserName = _solaceConfiguration.UserName,
                Password = _solaceConfiguration.Password
            };

            _session = context.CreateSession(sessionProps, null, sessionEventHandler);

            ReturnCode returnCode = _session.Connect();
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                _logger.Log(LogLevel.Information, "Session successfully connected.");
            }
            else
            {
                _logger.Log(LogLevel.Error, $"Error connecting, return code: {returnCode}");
            }
            // Initialize Solace Systems Messaging API with logging to console at Warning level            

            // This is the topic on Solace messaging router where a message is published
            // Must subscribe to it to receive messages
            _session.Subscribe(ContextFactory.Instance.CreateTopic("subscriptionRequest"), true);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        private void handleFlowEvent(object? sender, FlowEventArgs e)
        {
            _logger.Log(LogLevel.Information, e.ToString());
        }

        private void handleMessageEvent(object? sender, MessageEventArgs e)
        {
            var message = Encoding.ASCII.GetString(e.Message.BinaryAttachment);

            var resource = JsonSerializer.Deserialize<CreateResource>(message);

           // Flow.Ack(e.Message.ADMessageId);

            _channelWriter.TryWrite(resource!);

            _logger.Log(LogLevel.Information, message);
        }

        private void sessionEventHandler(object? sender, SessionEventArgs e)
        {
            _logger.Log(LogLevel.Information, e.ToString());
        }

    }



    public class TickReader : IHostedService
    {
        private readonly ChannelReader<CreateResource> _channelReader;
        private readonly ILogger<TickReader> _logger;

        public TickReader(ChannelReader<CreateResource> channelReader, ILogger<TickReader> logger)
        {
            _channelReader = channelReader;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            int counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var tick in _channelReader.ReadAllAsync(cancellationToken))
                    {
                        counter++;
                        //_logger.LogInformation("{Count}: {Tick}", counter, tick);
                        //_logger.ProcessForexTick(tick.Pair, tick.Ask, tick.Bid);
                    }
                }
                catch (ChannelClosedException)
                {
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}


