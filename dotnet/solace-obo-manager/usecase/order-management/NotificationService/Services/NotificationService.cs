using Contracts.Requests;
using NotificationService.Domain;
using NotificationService.Repositories;

namespace NotificationService.Services
{
    // Services/NotificationService.cs
    public class NotificationService(
        ICustomerResolver customerResolver,
        IEmailSender emailSender,
        ISmsGateway smsGateway,
        INotificationRepository repo,
        ILogger<NotificationService> logger) : INotificationService
    {
        private static readonly Dictionary<string, (string Subject, string Body)> Templates = new()
        {
            ["OrderConfirmed"] = (
                Subject: "Your order {{OrderId}} is confirmed",
                Body: "Hi {{CustomerName}}, your order {{OrderId}} totalling {{OrderTotal}} has been confirmed."
            ),
            ["OrderShipped"] = (
                Subject: "Your order {{OrderId}} has shipped",
                Body: "Hi {{CustomerName}}, your order {{OrderId}} is on its way."
            ),
            ["OrderDelivered"] = (
                Subject: "Your order {{OrderId}} has been delivered",
                Body: "Hi {{CustomerName}}, your order {{OrderId}} has been delivered."
            ),
            ["OrderCancelled"] = (
                Subject: "Your order {{OrderId}} has been cancelled",
                Body: "Hi {{CustomerName}}, your order {{OrderId}} has been cancelled."
            )
        };

        public async Task SendAsync(SendNotificationRequest request, CancellationToken ct)
        {
            var contact = await customerResolver.ResolveAsync(request.CustomerId, ct);

            if (contact is null)
            {
                logger.LogWarning(
                    "Could not resolve contact for customer {CustomerId} — skipping notification",
                    request.CustomerId);
                return;
            }

            if (!Templates.TryGetValue(request.EventType, out var template))
            {
                logger.LogWarning(
                    "No template found for event type {EventType}", request.EventType);
                return;
            }

            var subject = Render(template.Subject, contact, request);
            var body = Render(template.Body, contact, request);

            try
            {
                await emailSender.SendAsync(contact.EmailAddress, subject, body, ct);

                await repo.SaveAsync(
                    NotificationRecord.CreateSuccess(
                        request.CustomerId,
                        request.OrderId,
                        request.EventType,
                        NotificationChannel.Email), ct);

                logger.LogInformation(
                    "Sent {EventType} notification to customer {CustomerId}",
                    request.EventType, request.CustomerId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to send {EventType} notification to customer {CustomerId}",
                    request.EventType, request.CustomerId);

                await repo.SaveAsync(
                    NotificationRecord.CreateFailed(
                        request.CustomerId,
                        request.OrderId,
                        request.EventType,
                        NotificationChannel.Email,
                        ex.Message), ct);
            }
        }

        private static string Render(
            string template,
            CustomerContact contact,
            SendNotificationRequest request) =>
            template
                .Replace("{{CustomerName}}", contact.Name)
                .Replace("{{OrderId}}", request.OrderId.ToString())
                .Replace("{{OrderTotal}}", request.OrderTotal?.ToString("C") ?? string.Empty);
    }
}
