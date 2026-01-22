using Refit;
using SolaceOboManager.OAuthClient.Agent;
using SolaceOboManager.Shared;
using SolaceSystems.Solclient.Messaging;
using System.Text;
using System.Threading;

namespace SolaceOboManager.OAuthClient;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string? _solaceHost;
    private readonly string? _keycloakHost;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;

        _solaceHost = configuration["SOLACE_HOST"];
        _keycloakHost = "https+http://mykeycloak";// configuration["MYKEYCLOAK_HTTPS"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Usage
        var authClient = RestService.For<IKeycloakAgent>(_keycloakHost);
        

        var request = new PasswordGrantRequest
        {
            ClientId = "solace",
            Username = "test1",
            Password = "123",
            ClientSecret = "BhY28TyaleYlw4MpNsGmF8kyWZnY2C5H"
        };

        var tokenResponse = await authClient.GetToken("master", request);


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
            Host = _solaceHost,
            VPNName = "default",
            //UserName = "client",
            //Password = "password",
            AuthenticationScheme = SolaceSystems.Solclient.Messaging.AuthenticationSchemes.OAUTH2,
            OAuth2AccessToken = tokenResponse.AccessToken,
            SSLValidateCertificate = false,
            SSLTrustStoreDir = "../../certificates"
        };

        var session = context.CreateSession(sessionProps, messageEventHander, sessionEventHandler);

        ReturnCode returnCode = session.Connect();
        if (returnCode == ReturnCode.SOLCLIENT_OK)
        {
            Console.WriteLine("Session successfully connected.");
        }
        else
        {
            Console.WriteLine("Error connecting, return code: {0}", returnCode);
        }

        RequestSubscription(session);
    }

    private void RequestSubscription(ISession session)
    {
        var list = new List<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };

        foreach (var pair in list)
        {

            using (IMessage requestMessage = ContextFactory.Instance.CreateMessage())
            {
                requestMessage.Destination = ContextFactory.Instance.CreateTopic("subscriptionRequest");
                // Create the request content as a binary attachment


                var request = new SubscriptionRequest { ClientName = session.Properties.ClientName, Pair = pair };
                requestMessage.BinaryAttachment = Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(request));

                // Send the request message to the Solace messaging router
                Console.WriteLine("Sending request...");
                ReturnCode returnCode = session.Send(requestMessage);
                if (returnCode == ReturnCode.SOLCLIENT_OK)
                {
                    // Expecting reply as a binary attachment
                    Console.WriteLine("Subscription request sent successful!");
                }
                else
                {
                    Console.WriteLine("Request failed, return code: {0}", returnCode);
                }
            }
        }
    }

    private void sessionEventHandler(object? sender, SessionEventArgs e)
    {
        Console.WriteLine(e.ToString());
    }

    private int _messageCount = 1;

    private void messageEventHander(object? sender, MessageEventArgs e)
    {
        _logger.LogInformation("{Count}::{Destination} - {Message}", _messageCount, e.Message.Destination.Name, Encoding.ASCII.GetString(e.Message.BinaryAttachment));
        _messageCount++;
        //var payload = JsonSerializer.Deserialize<ForexTick>(Encoding.ASCII.GetString(e.Message.BinaryAttachment), opts);

        Task.Delay(2).GetAwaiter().GetResult();
    }
}
