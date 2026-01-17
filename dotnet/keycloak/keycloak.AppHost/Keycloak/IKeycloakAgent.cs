using keycloak.AppHost.Keycloak.Model;
using Refit;

namespace keycloak.AppHost.Keycloak
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
