namespace NotificationService.Services
{
    // Services/CustomerResolver.cs  — stub, replace with a real customer service call
    public class CustomerResolver(ILogger<CustomerResolver> logger) : ICustomerResolver
    {
        public Task<CustomerContact?> ResolveAsync(Guid customerId, CancellationToken ct)
        {
            // Replace with: HttpClient call to a Customer Service
            logger.LogInformation("Resolving contact for customer {CustomerId}", customerId);

            return Task.FromResult<CustomerContact?>(new CustomerContact(
                customerId,
                Name: "John Smith",
                EmailAddress: "john.smith@example.com",
                PhoneNumber: "+27821234567"));
        }
    }
}
