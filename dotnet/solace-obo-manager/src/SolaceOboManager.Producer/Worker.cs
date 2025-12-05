using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolaceOboManager.Producer;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // Initialize Solace Systems Messaging API with logging to console at Warning level
        ContextFactoryProperties cfp = new ContextFactoryProperties()
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
            VPNName = "default",
            UserName = "publisher",
            Password = "password"
        };

        var session = context.CreateSession(sessionProps, messageEventHander, sessionEventHandler);

        ReturnCode returnCode = session.Connect();
        if (returnCode == ReturnCode.SOLCLIENT_OK)
        {
            _logger.LogInformation("Session successfully connected.");
        }
        else
        {
            _logger.LogInformation("Error connecting, return code: {0}", returnCode);
        }


        ForexTicker forexTicker = new();
        forexTicker.OnTick += (tick) =>
        {
            SendMessage(session, tick);
        };
        await forexTicker.StartAsync(stoppingToken);
    }

    private void SendMessage(ISession session, ForexTick tick)
    {
        //Write it to redis before the send
        using (IMessage message = ContextFactory.Instance.CreateMessage())
        {
            var topic = ContextFactory.Instance.CreateTopic($"clientSubscribeTopic-{tick.Pair.Replace("/", "").ToLowerInvariant()}");
            message.Destination = topic;
            message.BinaryAttachment = Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(tick));
            message.ElidingEligible = true;
            var returnCode = session.Send(message);
        }
    }

    private void sessionEventHandler(object? sender, SessionEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private void messageEventHander(object? sender, MessageEventArgs e)
    {

    }
}
