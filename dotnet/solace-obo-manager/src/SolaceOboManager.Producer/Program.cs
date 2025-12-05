using SolaceOboManager.Producer;
using System.Net.NetworkInformation;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddHostedService<Worker>();
//builder.Services.AddHostedService<RegistrationWorker>();

var host = builder.Build();
host.Run();



