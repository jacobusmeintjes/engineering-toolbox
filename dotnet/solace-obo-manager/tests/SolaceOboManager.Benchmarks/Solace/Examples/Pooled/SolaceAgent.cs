using Microsoft.Extensions.ObjectPool;
using SolaceSystems.Solclient.Messaging;
using System.Collections.Concurrent;

namespace SolaceOboManager.Benchmarks.Solace.Examples.Pooled
{

    public class SolaceAgent
    {
        private readonly ObjectPool<IMessage> _messagePool;
        private readonly ConcurrentDictionary<string, ITopic> _topicDictionary = new();
        private ISession _session;

        public ISession Session
        {
            set
            {
                _session = value;
            }
        }

        public SolaceAgent()
        {
            var policy = new MessageObjectPolicy();
            _messagePool = new DefaultObjectPool<IMessage>(policy);
        }

        public void PublishMessage(string topic, byte[] attachment)
        {
            var message = _messagePool.Get();

            try
            {
                message.Destination = GetOrCreateTopic(topic);
                message.BinaryAttachment = attachment;
                message.ElidingEligible = true;
                message.DeliveryMode = MessageDeliveryMode.Direct;

                _ = _session.Send(message);
            }
            finally
            {
                _messagePool.Return(message);
            }
        }


        public void PublishMessageWithoutPropertiesBeingOverwritten(string topic, byte[] attachment)
        {
            var message = _messagePool.Get();

            try
            {
                message.Destination = GetOrCreateTopic(topic);
                message.BinaryAttachment = attachment;

                _ = _session.Send(message);
            }
            finally
            {
                _messagePool.Return(message);
            }
        }



        public void PublishMessageWithNewTopic(string topic, byte[] attachment)
        {
            var message = _messagePool.Get();

            try
            {
                message.Destination = ContextFactory.Instance.CreateTopic(topic);
                message.BinaryAttachment = attachment;
                message.ElidingEligible = true;
                message.DeliveryMode = MessageDeliveryMode.Direct;

                _ = _session.Send(message);
            }
            finally
            {
                _messagePool.Return(message);
            }
        }


        private ITopic GetOrCreateTopic(string topic)
        {

            if(_topicDictionary.TryGetValue(topic, out var existingTopic))
            {
                return existingTopic;
            }

            var newTopic = ContextFactory.Instance.CreateTopic(topic);
            _topicDictionary.TryAdd(topic, newTopic);

            return newTopic;
        }
    }
}
