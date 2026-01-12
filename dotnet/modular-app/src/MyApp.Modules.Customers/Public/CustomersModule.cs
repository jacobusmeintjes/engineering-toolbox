using MyApp.Modules.Customers.Internal;
using MyApp.Modules.Customers.Public.Dtos;

namespace MyApp.Modules.Customers.Public
{
    public interface ICustomersModule
    {
        Task<CustomerDto?> GetCustomerAsync(Guid customerId, CancellationToken ct = default);
        Task<CustomerDto?> GetCustomerByEmailAsync(string email, CancellationToken ct = default);
        Task<Guid> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default);
        Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default);
        Task<bool> IsCustomerActiveAsync(Guid customerId, CancellationToken ct = default);
        Task<CustomerCreditInfo> GetCustomerCreditInfoAsync(Guid customerId, CancellationToken ct = default);
    }

    public class CustomersModule : ICustomersModule
    {
        private readonly ICustomerService _customerService;

        public CustomersModule(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        public async Task<Guid> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
        {
            Internal.Models.Customer customer = new()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,               
            };

            if(request.BillingAndShippingAddressIsTheSame && request.BillingAddress is not null)
            {
                var address = new Internal.Models.Address
                {
                    Street = request.BillingAddress.Street,
                    City = request.BillingAddress.City,
                    State = request.BillingAddress.State,
                    ZipCode = request.BillingAddress.ZipCode,
                    Country = request.BillingAddress.Country
                };
                customer.WithBillingAddress(address);
                customer.WithShippingAddress(address);
            }
            else
            {
                if (request.BillingAddress is not null)
                {
                    customer.WithBillingAddress(new Internal.Models.Address
                    {
                        Street = request.BillingAddress.Street,
                        City = request.BillingAddress.City,
                        State = request.BillingAddress.State,
                        ZipCode = request.BillingAddress.ZipCode,
                        Country = request.BillingAddress.Country
                    });
                }
                if (request.ShippingAddress is not null)
                {
                    customer.WithShippingAddress(new Internal.Models.Address
                    {
                        Street = request.ShippingAddress.Street,
                        City = request.ShippingAddress.City,
                        State = request.ShippingAddress.State,
                        ZipCode = request.ShippingAddress.ZipCode,
                        Country = request.ShippingAddress.Country
                    });
                }
            }

            return await _customerService.CreateCustomerAsync(customer, ct);
        }

        public Task<CustomerDto?> GetCustomerAsync(Guid customerId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerDto?> GetCustomerByEmailAsync(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerCreditInfo> GetCustomerCreditInfoAsync(Guid customerId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCustomerActiveAsync(Guid customerId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
