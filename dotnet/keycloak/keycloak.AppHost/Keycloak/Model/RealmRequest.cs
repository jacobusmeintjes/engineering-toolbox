using System.Text.Json.Serialization;

namespace keycloak.AppHost.Keycloak.Model
{
    class RealmRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("realm")]
        public string Realm { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName => Realm;

        [JsonPropertyName("displayNameHtml")]
        public string DisplayNameHtml => $"<div class=\"kc-logo-text\"><span>{Realm}</span></div>";

    }


}
