using Microsoft.Extensions.ObjectPool;
using SolaceSystems.Solclient.Messaging;

namespace SolaceOboManager.Benchmarks.Solace.Examples.Pooled
{
    public class MessageObjectPolicy : PooledObjectPolicy<IMessage>
    {
        public override IMessage Create()
        {
            var message = ContextFactory.Instance.CreateMessage();
            message.DeliveryMode = MessageDeliveryMode.Direct;
            message.ElidingEligible = true;

            return message;
        }

        public override bool Return(IMessage obj)
        {
            obj.Reset();
            return true;
        }
    }