namespace NotificationService.Services
{
    // Services/SmsGateway.cs  — stub, replace with Twilio SDK
    public class SmsGateway(ILogger<SmsGateway> logger) : ISmsGateway
    {
        public Task SendAsync(
            string toNumber,
            string message,
            CancellationToken ct)
        {
            // Replace with: TwilioClient.Messages.CreateAsync(...)
            logger.LogInformation(
                "Sending SMS to {ToNumber}", toNumber);

            return Task.CompletedTask;
        }
    }
}
