using Solace.Manager.UI.HostedServices;
using SolaceOboManager.Manager;
using System.Text.Json.Serialization;
using System.Threading.Channels;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var solaceConfiguration = builder.Configuration.GetSection(nameof(SolaceConfiguration)).Get<SolaceConfiguration>();

builder.Services.AddSingleton(Channel.CreateUnbounded<CreateResource>(new UnboundedChannelOptions() { SingleReader = true }));
builder.Services.AddSingleton(svc => svc.GetRequiredService<Channel<CreateResource>>().Reader);
builder.Services.AddSingleton(svc => svc.GetRequiredService<Channel<CreateResource>>().Writer);

builder.Services.AddSingleton(solaceConfiguration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();



public class SolaceServiceConfiguration
{
    public string[] Admin { get; set; }

    public string? AdminUri => Admin?.FirstOrDefault();

    public string[] Tcp { get; set; }

    public string? TcpUri => Tcp?.FirstOrDefault();

    public string[] Ws { get; set; }
    public string? WsUri => Ws?.FirstOrDefault();

    public string[] Mqtt { get; set; }

    public string? MqttUri => Mqtt?.FirstOrDefault();
}
