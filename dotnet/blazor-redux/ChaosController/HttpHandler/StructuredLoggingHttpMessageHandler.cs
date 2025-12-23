using System.Diagnostics;

namespace ChaosController.HttpHandler
{
    public class ConsoleLoggingHttpMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"═══ HTTP REQUEST ═══");
            Console.ResetColor();
            Console.WriteLine($"→ {request.Method} {request.RequestUri}");

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Headers: {string.Join(", ", request.Headers.Select(h => h.Key))}");
                Console.WriteLine($"Body: {requestBody}");
                Console.ResetColor();
            }

            var stopwatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            var color = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
            Console.ForegroundColor = color;
            Console.WriteLine($"═══ HTTP RESPONSE ({stopwatch.ElapsedMilliseconds}ms) ═══");
            Console.ResetColor();
            Console.WriteLine($"← {(int)response.StatusCode} {response.ReasonPhrase}");

            if (response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => h.Key))}");
                Console.WriteLine($"Body: {responseBody}");
                Console.ResetColor();
            }

            Console.WriteLine();
            return response;
        }
    }

}
