
namespace MyApp.Modules.Customers.Public.Dtos
{
    public sealed record CustomerDto(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        CustomerStatus Status,
        AddressDto? BillingAddress,
        AddressDto? ShippingAddress,
        DateTime CreatedAt,
        DateTime? LastModifiedAt);

    public sealed record AddressDto(
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        string ZipCode)
    {
        public AddressDto() : this(
            Street: string.Empty,
            City: string.Empty,
            State: string.Empty,
            PostalCode: string.Empty,
            Country: string.Empty,
            ZipCode: string.Empty)
        {
            
        }
    }
    

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended,
        Deleted
    }

    public sealed record CustomerCreditInfo(
        Guid CustomerId,
        decimal CreditLimit,
        decimal CurrentBalance,
        bool IsInGoodStanding);

    // MyApp.Modules.Customers.Public/CreateCustomerRequest.cs
    public sealed record CreateCustomerRequest(
        string Email,
        string FirstName,
        string LastName,
        AddressDto? BillingAddress,
        AddressDto? ShippingAddress,
        bool BillingAndShippingAddressIsTheSame)
    {
        public CreateCustomerRequest() : this(
            Email: string.Empty,
            FirstName: string.Empty,
            LastName: string.Empty,
            BillingAddress: null,
            ShippingAddress: null,
            BillingAndShippingAddressIsTheSame: false)
        {
            
        }
    }

    // MyApp.Modules.Customers.Public/UpdateCustomerRequest.cs
    public sealed record UpdateCustomerRequest(
        string? FirstName = null,
        string? LastName = null,
        AddressDto? BillingAddress = null,
        AddressDto? ShippingAddress = null);
}
