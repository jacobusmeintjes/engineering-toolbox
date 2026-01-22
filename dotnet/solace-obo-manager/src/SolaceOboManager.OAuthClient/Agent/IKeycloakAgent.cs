using Refit;

namespace SolaceOboManager.OAuthClient.Agent
{
    interface IKeycloakAgent
    {
        [Post("/realms/{realm}/protocol/openid-connect/token")]
        Task<TokenResponse> GetToken(string realm, [Body(BodySerializationMethod.UrlEncoded)] PasswordGrantRequest request);

    }


}
