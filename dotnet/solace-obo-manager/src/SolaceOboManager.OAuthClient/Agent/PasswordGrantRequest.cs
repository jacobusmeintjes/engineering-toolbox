using Refit;

namespace SolaceOboManager.OAuthClient.Agent
{
    //{ "access_token":"",
    //        "expires_in":60,
    //        "refresh_expires_in":1800,
    //        "refresh_token":"",
    //        "token_type":"Bearer",
    //        "not-before-policy":0,
    //        "session_state":"619c6a5b-fb01-11ff-a2be-9bfe5782fa8b",
    //        "scope":"profile email"}

    public class PasswordGrantRequest
    {
        [AliasAs("grant_type")]
        public string GrantType { get; set; } = "password";

        [AliasAs("client_id")]
        public string ClientId { get; set; } = string.Empty;

        [AliasAs("username")]
        public string Username { get; set; } = string.Empty;

        [AliasAs("password")]
        public string Password { get; set; } = string.Empty;

        [AliasAs("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;
    }


}
