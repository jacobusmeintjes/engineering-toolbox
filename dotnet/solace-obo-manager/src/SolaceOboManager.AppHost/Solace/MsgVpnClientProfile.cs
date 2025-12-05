using System.Text.Json.Serialization;

namespace SolaceOboManager.AdminService.SolaceConfig;

public class MsgVpnClientProfile
{
    [JsonPropertyName("allowGuaranteedMsgReceiveEnabled")]
    public bool AllowGuaranteedMsgReceiveEnabled { get; } = true;

    [JsonPropertyName("allowGuaranteedMsgSendEnabled")]
    public bool AllowGuaranteedMsgSendEnabled { get; set; } = true;

    [JsonPropertyName("clientProfileName")]
    public string ClientProfileName { get; set; }

    [JsonPropertyName("compressionEnabled")]
    public bool CompressionEnabled { get; set; } = true;

    [JsonPropertyName("elidingEnabled")]
    public bool ElidingEnabled { get; set; } = false;

    [JsonPropertyName("elidingDelay")]
    public long ElidingDelay { get; set; } = 0;

    [JsonPropertyName("elidingMaxTopicCount")]
    public long ElidingMaxTopicCount { get; set; } = 256;

    [JsonPropertyName("tlsAllowDowngradeToPlainTextEnabled")]
    public bool TlsAllowDowngradeToPlainTextEnabled { get; set; } = true;
}