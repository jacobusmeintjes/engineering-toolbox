using SolaceOboManager.Client;
using SolaceOboManager.Client.Model;
using System.Threading.Channels;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
