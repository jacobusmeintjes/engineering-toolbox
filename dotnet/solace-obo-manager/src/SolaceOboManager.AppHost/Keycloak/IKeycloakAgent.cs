using SolaceOboManager.AppHost.Keycloak.Model;
using Refit;

namespace SolaceOboManager.AppHost.Keycloak
{
    interface IKeycloakAgent
    {
        [Post("/realms/{realm}/protocol/openid-connect/token")]
        Task<TokenResponse> GetToken(string realm, [Body(BodySerializationMethod.UrlEncoded)] PasswordGrantRequest request);

        [Get("/admin/realms")]
        Task<List<KeycloackRealm>> GetRealmInformation();

        [Post("/admin/realms")]
        Task AddRealm([Body] RealmRequest request);
    }


}
