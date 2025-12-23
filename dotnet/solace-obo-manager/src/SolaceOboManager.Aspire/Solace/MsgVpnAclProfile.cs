using System.Text.Json.Serialization;

namespace SolaceOboManager.Aspire.Solace;

internal class MsgVpnAclProfile
{
    [JsonPropertyName("aclProfileName")]
    public string AclProfileName { get; set; }

    [JsonPropertyName("clientConnectDefaultAction")]
    public string ClientConnectDefaultAction { get; set; } = "disallow";

    [JsonPropertyName("msgVpnName")]
    public string VpnName { get; set; } = "default";
    
    [JsonPropertyName("publishTopicDefaultAction")] 
    public string PublishTopicDefaultAction { get; set; } = "disallow";
    
    [JsonPropertyName("subscribeShareNameDefaultAction")] 
    public string SubscribeShareNameDefaultAction { get; set; } = "allow";
    
    [JsonPropertyName("subscribeTopicDefaultAction")] 
    public string SubscribeTopicDefaultAction { get; set; } = "disallow";
    
}
