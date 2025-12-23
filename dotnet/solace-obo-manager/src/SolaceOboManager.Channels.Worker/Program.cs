using SolaceOboManager.Channels.Worker;
using SolaceOboManager.Shared.Channels;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<ObservableChannel<string>>(
                sp => new ObservableChannel<string>("test-orders", 1000));


builder.Services.AddHostedService<ObservableChannelWriterWorker>();
builder.Services.AddHostedService<ObservableChannelReaderWorker>();

var host = builder.Build();
host.Run();
