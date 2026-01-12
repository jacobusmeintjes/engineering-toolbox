

namespace MyApp.Modules.Customers.Internal.Models
{
    public sealed class Customer
    {
        public required Guid Id { get; init; }
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public Address? BillingAddress { get; private set; }
        public Address? ShippingAddress { get; private set; }

        public static Customer Create(Guid id, string email, string firstName, string lastName)
        {
            return new Customer
            {
                Id = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        public void WithBillingAddress(Address address)
        {
            this.BillingAddress = address;
        }

        public void WithShippingAddress(Address address)
        {
            this.ShippingAddress = address;
        }
    }

    public sealed class Address
    {
        public required string Street { get; init; }
        public required string City { get; init; }
        public required string State { get; init; }
        public required string ZipCode { get; init; }
        public required string Country { get; init; }

        public static Address Create(string street, string city, string state, string zipCode, string country)
        {
            return new Address
            {
                Street = street,
                City = city,
                State = state,
                ZipCode = zipCode,
                Country = country
            };
        }
    }
}
