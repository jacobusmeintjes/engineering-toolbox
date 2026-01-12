using MyApp.Modules.Customers.Internal.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MyApp.Modules.Customers.Internal
{
    public class CustomerService : ICustomerService
    {
        public ConcurrentDictionary<Guid, Customer> Customers = new ConcurrentDictionary<Guid, Customer>();

        public CustomerService() { }
        public Task<Guid> CreateCustomerAsync(Customer customer, CancellationToken ct)
        {
            Customers[customer.Id] = customer;
            return Task.FromResult(customer.Id);
        }
    }

    public interface ICustomerService
    {
        Task<Guid> CreateCustomerAsync(Customer customer, CancellationToken ct);
    }
}
