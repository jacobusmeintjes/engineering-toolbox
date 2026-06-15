namespace NotificationService.Services
{

    // Services/ICustomerResolver.cs  — looks up customer contact details
    public interface ICustomerResolver
    {
        Task<CustomerContact?> ResolveAsync(Guid customerId, CancellationToken ct);
    }
}
