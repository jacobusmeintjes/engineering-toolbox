using SolaceSystems.Solclient.Messaging;

namespace SolaceOboManager.Benchmarks.Solace.Examples.Standard
{

    public class SolaceAgent
    {

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
        }

        public void PublishMessage(string topic, byte[] attachment)
        {
            var message = ContextFactory.Instance.CreateMessage();
            message.DeliveryMode = MessageDeliveryMode.Direct;
            message.ElidingEligible = true;

            message.Destination = ContextFactory.Instance.CreateTopic(topic);
            message.BinaryAttachment = attachment;

            _ = _session.Send(message);
        }
    }
}
