var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight(opts => { opts.WithHostPort(16379); });

var orleans = builder.AddOrleans("default")
    .WithClustering(redis);

var silo = builder.AddProject<Projects.orleans_hive_server>("orleans-hive-server")
    .WithReference(orleans)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReplicas(3);


var client = builder.AddProject<Projects.orleans_hive_client>("orleans-hive-client")
    .WithReference(orleans)
    .WithReference(redis)
    .WaitFor(redis);


builder.Build().Run();
