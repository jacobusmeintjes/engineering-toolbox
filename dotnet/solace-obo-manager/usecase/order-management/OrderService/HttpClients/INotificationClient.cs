using Contracts.Requests;

namespace OrderService.HttpClients
{
    // HttpClients/INotificationClient.cs
    public interface INotificationClient
    {
        Task SendAsync(SendNotificationRequest request, CancellationToken ct);
    }

    // HttpClients/NotificationClient.cs
    public class NotificationClient(HttpClient http) : INotificationClient
    {
        public async Task SendAsync(SendNotificationRequest request, CancellationToken ct)
        {
            var response = await http.PostAsJsonAsync("notifications/send", request, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
