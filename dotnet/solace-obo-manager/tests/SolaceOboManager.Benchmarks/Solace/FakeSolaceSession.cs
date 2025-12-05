using SolaceSystems.Solclient.Messaging;
using SolaceSystems.Solclient.Messaging.Cache;
using System;
using System.Collections.Generic;

namespace SolaceOboManager.Benchmarks.Solace
{
    public class FakeSolaceSession : ISession
    {
        public SessionProperties Properties => throw new NotImplementedException();

        public IList<IFlow> Flows => throw new NotImplementedException();

        public ReturnCode ClearStats()
        {
            throw new NotImplementedException();
        }

        public ReturnCode Connect()
        {
            throw new NotImplementedException();
        }

        public IBrowser CreateBrowser(IEndpoint endpointToBrowse, BrowserProperties browserProperties)
        {
            throw new NotImplementedException();
        }

        public ICacheSession CreateCacheSession(CacheSessionProperties cacheSessionProperties)
        {
            throw new NotImplementedException();
        }

        public IDispatchTarget CreateDispatchTarget(ISubscription subscription, EventHandler<MessageEventArgs> messageCallback)
        {
            throw new NotImplementedException();
        }

        public IFlow CreateFlow(FlowProperties flowProperties, IEndpoint endPoint, ISubscription subscription, EventHandler<MessageEventArgs> messageEventHandler, EventHandler<FlowEventArgs> flowEventHandler)
        {
            throw new NotImplementedException();
        }

        public IFlow CreateFlow(FlowProperties flowProperties, IEndpoint endPoint, ISubscription subscription, EventHandler<MessageEventArgs> messageEventHandler, EventHandler<FlowEventArgs> flowEventHandler, EndpointProperties endPointProperties)
        {
            throw new NotImplementedException();
        }

        public IMessage CreateMessage()
        {
            throw new NotImplementedException();
        }

        public ITopicEndpoint CreateNonDurableTopicEndpoint()
        {
            throw new NotImplementedException();
        }

        public ITopicEndpoint CreateNonDurableTopicEndpoint(string name)
        {
            throw new NotImplementedException();
        }

        public IQueue CreateTemporaryQueue()
        {
            throw new NotImplementedException();
        }

        public IQueue CreateTemporaryQueue(string name)
        {
            throw new NotImplementedException();
        }

        public ITopic CreateTemporaryTopic()
        {
            throw new NotImplementedException();
        }

        public ITransactedSession CreateTransactedSession(TransactedSessionProperties props)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Deprovision(IEndpoint endpoint, int flags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ICapability GetCapability(CapabilityType capabilityType)
        {
            throw new NotImplementedException();
        }

        public IContext GetContext()
        {
            throw new NotImplementedException();
        }

        public object GetProperty(SessionProperties.PROPERTY sessionProperty)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Stats_Rx, long> GetRxStats()
        {
            throw new NotImplementedException();
        }

        public IDictionary<Stats_Tx, long> GetTxStats()
        {
            throw new NotImplementedException();
        }

        public bool IsCapable(CapabilityType capabilityType)
        {
            throw new NotImplementedException();
        }

        public ReturnCode ModifyClientInfo(SessionProperties.PROPERTY sessionProperty, object value, int flags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode ModifyProperty(SessionProperties.PROPERTY sessionProperty, object value)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Provision(IEndpoint endpoint, EndpointProperties props, int flags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Send(IMessage message)
        {
            return ReturnCode.SOLCLIENT_OK;
        }

        public ReturnCode Send(IMessage[] messages, int offset, int length, out int messagesSent)
        {
            throw new NotImplementedException();
        }

        public ReturnCode SendReply(IMessage messageToReplyTo, IMessage replyMessage)
        {
            throw new NotImplementedException();
        }

        public ReturnCode SendRequest(IMessage requestMessage, out IMessage replyMessage, int timeoutInMsecs)
        {
            throw new NotImplementedException();
        }

        public ReturnCode SetClientDescription(string clientDescription)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Subscribe(ISubscription subscription, bool waitForConfirm)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Subscribe(IEndpoint endpoint, ISubscription subscription, int subscribeFlags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Subscribe(IDispatchTarget dispatchTarget, int flags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Unsubscribe(ISubscription subscription, bool waitForConfirm)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Unsubscribe(IEndpoint endpoint, ISubscription subscription, int subscribeFlags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Unsubscribe(ITopicEndpoint dte, int correlationId)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Unsubscribe(ITopicEndpoint dte, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode Unsubscribe(IDispatchTarget dispatchTarget, int flags, object correlationKey)
        {
            throw new NotImplementedException();
        }

        public ReturnCode ValidateTopic(string topicName)
        {
            throw new NotImplementedException();
        }
    }
}


