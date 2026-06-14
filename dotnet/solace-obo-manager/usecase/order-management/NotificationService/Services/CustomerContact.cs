namespace NotificationService.Services
{
    // Services/CustomerContact.cs
    public record CustomerContact(
        Guid CustomerId,
        string Name,
        string EmailAddress,
        string? PhoneNumber);
}
