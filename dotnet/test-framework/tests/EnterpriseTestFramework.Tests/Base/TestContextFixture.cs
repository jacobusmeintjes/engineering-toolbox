using EnterpriseFramework.Core.Specifications;
using EnterpriseFramework.Core.Utilities;
using RestAssured.Request.Builders;

namespace EnterpriseFramework.Tests.Base
{
    public sealed class TestContextFixture : IAsyncLifetime
    {
        public RequestSpecification ApiSpecification { get; private set; } = null!;

        public ValueTask DisposeAsync()
        {
            // Perform global teardowns here if reequired (e.g. database connection closing)
            return ValueTask.CompletedTask;
        }

        public ValueTask InitializeAsync()
        {
            // Thread-safe fetch of your cached enterprise JWT token
            string token = ConfigAndTokenManager.GetValidToken();

            // Generate your immutable global HTTP base spec configuration profile
            ApiSpecification = BaseSpecification.GetCommonSpecification(ConfigAndTokenManager.BaseUrl, token);

            return ValueTask.CompletedTask;
        }
    }
}
