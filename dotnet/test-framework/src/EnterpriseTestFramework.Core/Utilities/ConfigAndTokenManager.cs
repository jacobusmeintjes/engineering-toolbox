using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using static RestAssured.Dsl;

namespace EnterpriseFramework.Core.Utilities
{
    public static class ConfigAndTokenManager
    {

        private static readonly IConfiguration _config;
        private static string _cachedToken = string.Empty;
        private static DateTime _tokenExpiry = DateTime.MinValue;
        private static readonly object _lock = new();

        static ConfigAndTokenManager()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "QA";

            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static string BaseUrl => _config["ApiSettings:BaseUrl"] ?? "https://reqres.in";
        public static string AuthUrl => _config["ApiSettings:AuthUrl"] ?? "https://reqres.in";

        public static string GetValidToken()
        {
            lock (_lock)
            {
                // Returns cached token if it is still valid
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return _cachedToken;
                }

                // Simulating fetching a new token from the authorization gateway
                var authPayload = new { email = "eve.holt@reqres.in", password = "cityslicka" };

                var response = Given()
                    .Header("Content-Type", "application/json")
                    .Body(authPayload)
                .When()
                    .Post(AuthUrl);

                //_cachedToken = response.Extract().Path("token").ToString() ?? "";
                _cachedToken = "Blue";
                _tokenExpiry = DateTime.UtcNow.AddMinutes(55); // Cache for 55 minutes

                return _cachedToken;
            }
        }
    }
}
