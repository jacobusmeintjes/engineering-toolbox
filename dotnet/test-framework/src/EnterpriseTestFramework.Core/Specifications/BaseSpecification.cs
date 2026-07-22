using RestAssured.Request.Builders;

namespace EnterpriseFramework.Core.Specifications
{
    public static class BaseSpecification
    {
        public static RequestSpecification GetCommonSpecification(string baseUrl, string token)
        {
            return new RequestSpecBuilder()
                .WithBaseUri(baseUrl)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accept", "application/json")
                // Injects a unique ID to trace the request across microservices
                .WithHeader("X-Correlation-ID", Guid.NewGuid().ToString())
                .WithOAuth2(token)
                .Build();
        }
    }
}
