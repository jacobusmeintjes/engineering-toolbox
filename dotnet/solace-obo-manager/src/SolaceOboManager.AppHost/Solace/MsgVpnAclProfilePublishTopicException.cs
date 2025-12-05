using System.Text.Json.Serialization;

namespace SolaceOboManager.AdminService.SolaceConfig;

public class MsgVpnAclProfilePublishTopicException
{
    [JsonPropertyName("aclProfileName")]
    public string AclProfileName { get; set; }

    [JsonPropertyName("msgVpnName")]
    public string VpnName { get; set; } = "default";

    [JsonPropertyName("publishTopicException")]
    public string PublishTopicException { get; set; }

    /// <summary>
    /// The syntax of the topic for the exception to the default action taken. The allowed values and their meaning are:
    /// "smf" - Topic uses SMF syntax.
    /// "mqtt" - Topic uses MQTT syntax.
    /// </summary>
    [JsonPropertyName("publishTopicExceptionSyntax")]
    public string PublishTopicExceptionSyntax { get; set; } = "smf";

}
