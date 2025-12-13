using System.Text.Json.Serialization;

namespace SolaceOboManager.Aspire.Solace;


internal class MsgVpnClientUsername
{
    [JsonPropertyName("aclProfileName")]
    public string AclProfileName { get; set; } = "default";

    [JsonPropertyName("clientProfileName")]
    public string ProfileName { get; set; } = "default";

    [JsonPropertyName("clientUsername")]
    public string Username { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("guaranteedEndpointPermissionOverrideEnabled")]
    public bool GuaranteedEndpointPermissionOverrideEnabled { get; set; } = false;

    [JsonPropertyName("msgVpnName")]
    public string VpnName { get; set; } = "default";

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("subscriptionManagerEnabled")]
    public bool SubscriptionManagerEnabled { get; set; } = false;
}
