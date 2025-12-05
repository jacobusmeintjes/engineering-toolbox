using SolaceSystems.Solclient.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolaceOboManager.Benchmarks.Solace
{

    public static class SessionFactory
    {
        public static ISession CreateSession(string username, string password,
            EventHandler<ContextEventArgs> contextHandler = null,
            EventHandler<MessageEventArgs> messageHandler = null,
            EventHandler<SessionEventArgs> sessionHandler = null)
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
                Host = "tcp://localhost:55554",
                VPNName = "default",
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
    }
}
