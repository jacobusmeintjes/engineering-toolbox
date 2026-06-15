namespace NotificationService.Domain
{
    // Domain/NotificationTemplate.cs
    public class NotificationTemplate
    {
        public Guid Id { get; private set; }
        public string EventType { get; private set; } = default!;
        public string Subject { get; private set; } = default!;
        public string BodyTemplate { get; private set; } = default!;
        public NotificationChannel Channel { get; private set; }

        private NotificationTemplate() { }  // EF

        public string RenderSubject(NotificationContext ctx) =>
            Subject.Replace("{{OrderId}}", ctx.OrderId.ToString());

        public string RenderBody(NotificationContext ctx) =>
            BodyTemplate
                .Replace("{{OrderId}}", ctx.OrderId.ToString())
                .Replace("{{OrderTotal}}", ctx.OrderTotal?.ToString("C") ?? string.Empty)
                .Replace("{{CustomerName}}", ctx.CustomerName ?? string.Empty);
    }
}
