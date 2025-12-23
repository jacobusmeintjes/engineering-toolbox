using BenchmarkDotNet.Attributes;
using SolaceOboManager.Benchmarks.Solace;

namespace SolaceOboManager.Benchmarks
{
    [MemoryDiagnoser]//, Config(typeof(Config))]
    public class SolaceMessageBenchmarks
    {
        private readonly Solace.Examples.Standard.SolaceAgent _standardAgent = new Solace.Examples.Standard.SolaceAgent();
        private readonly Solace.Examples.Pooled.SolaceAgent _pooledAgent = new Solace.Examples.Pooled.SolaceAgent();

        private const string _topic = "topic";
        private readonly byte[] _attachment = [];

        [GlobalSetup]
        public void Setup()
        {
            _standardAgent.Session = new FakeSolaceSession();
            _pooledAgent.Session = new FakeSolaceSession();
        }

        [Benchmark]
        public void Standard_MessageSend()
        {
            _standardAgent.PublishMessage(_topic, _attachment);
        }


        [Benchmark]
        public void Pooled_MessageSend()
        {
            _pooledAgent.PublishMessage(_topic, _attachment);
        }

        [Benchmark]
        public void Pooled_MessageSendWithNewTopicCreated()
        {
            _pooledAgent.PublishMessageWithNewTopic(_topic, _attachment);
        }


        [Benchmark]
        public void Pooled_MessageSendWithoutPropertiesBeingOverwrittenCreated()
        {
            _pooledAgent.PublishMessageWithoutPropertiesBeingOverwritten(_topic, _attachment);
        }
    } 
}


