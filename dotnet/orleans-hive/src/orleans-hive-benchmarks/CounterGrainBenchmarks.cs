using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using orleans_hive_shared;
using System.Net;

namespace orleans_hive_benchmarks
{
    [Config(typeof(InProcessConfig))]
    //[MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    public class CounterGrainBenchmarks
    {
        private readonly ConsoleCancelEventHandler _onCancelEvent;
        private readonly List<IHost> hosts = new List<IHost>();
        private readonly ICounterGrain grain;
        private readonly IClusterClient client;
        private readonly IHost clientHost;

        public CounterGrainBenchmarks() : this(numSilos: 1, startClient: true)
        {
        }

        public CounterGrainBenchmarks(int numSilos, bool startClient, bool grainsOnSecondariesOnly = false)
        {
            for (var i = 0; i < numSilos; ++i)
            {
                var primary = i == 0 ? null : new IPEndPoint(IPAddress.Loopback, 11111);
                var hostBuilder = new HostBuilder().UseOrleans((ctx, siloBuilder) =>
                {
                    siloBuilder.UseLocalhostClustering(
                        siloPort: 11111 + i,
                        gatewayPort: 30000 + i,
                        primarySiloEndpoint: primary);

                    siloBuilder.AddMemoryGrainStorageAsDefault();

                    if (i == 0 && grainsOnSecondariesOnly)
                    {
                        siloBuilder.Configure<GrainTypeOptions>(options => options.Classes.Remove(typeof(ICounterGrain)));
                    }
                });

                var host = hostBuilder.Build();

                host.StartAsync().GetAwaiter().GetResult();
                this.hosts.Add(host);
            }

            if (grainsOnSecondariesOnly) Thread.Sleep(4000);

            if (startClient)
            {
                var hostBuilder = new HostBuilder().UseOrleansClient((ctx, clientBuilder) =>
                {
                    if (numSilos == 1)
                    {
                        clientBuilder.UseLocalhostClustering();
                    }
                    else
                    {
                        var gateways = Enumerable.Range(30000, numSilos).Select(i => new IPEndPoint(IPAddress.Loopback, i)).ToArray();
                        clientBuilder.UseStaticClustering(gateways);                        
                    }
                });

                this.clientHost = hostBuilder.Build();
                this.clientHost.StartAsync().GetAwaiter().GetResult();

                this.client = this.clientHost.Services.GetRequiredService<IClusterClient>();
                this.grain = this.client.GetGrain<ICounterGrain>(Guid.NewGuid().GetHashCode().ToString());
            }
        }

        // Config forces InProcess toolchain to avoid Orleans codegen conflict
        private class InProcessConfig : ManualConfig
        {
            public InProcessConfig() =>
                AddJob(Job.ShortRun
                    .WithLaunchCount(1)
                    .WithToolchain(InProcessEmitToolchain.Instance))
                   // .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                ;
        }


        [Benchmark(Baseline = true)]
        public async Task IncrementCounter()
        {            
            await this.grain.Increment();
        }


        [Benchmark]
        public async Task IncrementCounterWithGrainFactoryGet()
        {
            var mygrain = this.client.GetGrain<ICounterGrain>(Guid.NewGuid().GetHashCode().ToString());
            await mygrain.Increment();
        }


        //[Benchmark]
        //public async Task<int> Increment_UniqueGrainPerCall()
        //{
        //    // New grain key each time — measures activation overhead
        //    var grain = _cluster.Client
        //        .GetGrain<ICounterGrain>(Random.Shared.NextInt64().ToString());
        //    return await grain.Increment();
        //}

        //[Benchmark]
        //public async Task Increment_Concurrent()
        //{
        //    // 10 concurrent calls to the same grain — shows single-threaded actor model
        //    var tasks = Enumerable.Range(0, 10)
        //        .Select(_ => _grain.Increment());
        //    await Task.WhenAll(tasks);
        //}
    }
}
