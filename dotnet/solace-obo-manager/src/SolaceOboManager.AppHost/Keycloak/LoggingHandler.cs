using Microsoft.Extensions.Logging;

namespace SolaceOboManager.AppHost.Keycloak
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;

        public LoggingHandler(ILogger<LoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Log request
            _logger.LogInformation("HTTP {Method} {Uri}", request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Request Body: {Body}", requestBody);
            }

            // Send request
            var response = await base.SendAsync(request, cancellationToken);

            // Log response
            _logger.LogInformation("HTTP {StatusCode} from {Uri}",
                (int)response.StatusCode,
                request.RequestUri);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response Body: {Body}", responseBody);

            return response;
        }
    }


}
