using Microsoft.Extensions.DependencyInjection;
using SolaceSystems.Solclient.Messaging;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace SolaceOboManager.Benchmarks.Solace
{
    public record SolaceMessage(string TopicName, byte[] binaryAttachment, DateTime ReceivedAt);
    public record SolaceSessionEvent(string Event, string Description);
    public record SolaceContextEvent(int ResponseCode, string ErrorStr, Exception? Exception);

    public static class SolaceMessageClientExtensions
    {
        public static IServiceCollection AddSolace(this IServiceCollection services)
        {
            services.AddSingleton<SessionFactory>();
            return services;
        }
    }


    public class SessionFactory
    {
        public IObservable<SolaceMessage> Messages => _messages.AsObservable();
        public IObservable<SolaceSessionEvent> SessionEvents => _sessionEvents.AsObservable();
        public IObservable<SolaceContextEvent> ContextEvents => _contextEvents.AsObservable();
        public IObservable<bool> IsConnected => _isConnected.AsObservable();

        private readonly Subject<SolaceMessage> _messages;
        private readonly Subject<SolaceSessionEvent> _sessionEvents;
        private readonly Subject<SolaceContextEvent> _contextEvents;

        private readonly ReplaySubject<bool> _isConnected = new(1);

        public SessionFactory()
        {
            _messages = new Subject<SolaceMessage>();
            _sessionEvents = new Subject<SolaceSessionEvent>();
            _contextEvents = new Subject<SolaceContextEvent>();
        }

        public ISession CreateSession(string host, string vpnName, string username, string password)
        {
            // Initialize Solace Systems Messaging API with logging to console at Warning level
            ContextFactoryProperties cfp = new ContextFactoryProperties()
            {
                SolClientLogLevel = SolLogLevel.Warning
            };
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);

            IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), contextHandler);

            // Create session properties
            SessionProperties sessionProps = new SessionProperties()
            {
                Host = host,
                VPNName = vpnName,
                UserName = username,
                Password = password
            };

            var session = context.CreateSession(sessionProps, messageHandler, sessionHandler);

            ReturnCode returnCode = session.Connect();
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                Console.WriteLine("Session successfully connected.");
            }
            else
            {
                Console.WriteLine("Error connecting, return code: {0}", returnCode);
            }

            return session;
        }

        private void sessionHandler(object? sender, SessionEventArgs e)
        {
            var status = e.Event.ToString();

            var solaceSessionEvent = new SolaceSessionEvent(status, e.Info);
            _sessionEvents.OnNext(solaceSessionEvent);
            _isConnected.OnNext(status.Equals("UPNOTICE", StringComparison.InvariantCultureIgnoreCase));
        }

        private void messageHandler(object? sender, MessageEventArgs e)
        {
            var message = e.Message;
            var solaceMessage = new SolaceMessage(message.Destination.Name, message.BinaryAttachment, DateTime.UtcNow);

            _messages.OnNext(solaceMessage);
        }

        private void contextHandler(object? sender, ContextEventArgs e)
        {
            var solaceContextEvent = new SolaceContextEvent(e.ErrorInfo.ResponseCode, e.ErrorInfo.ErrorStr, e.Exception);
            _contextEvents.OnNext(solaceContextEvent);
        }
    }
}
