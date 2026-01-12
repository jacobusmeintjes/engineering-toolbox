using Bogus;
using MyApp.Modules.Customers.Public;
using MyApp.Modules.Customers.Public.Dtos;
using MyApp.Modules.Orders.Public;
using MyApp.Modules.Orders.Public.Dtos;

namespace MyApp
{
    public class Worker(IOrdersModule ordersModule, ICustomersModule customersModule, ILogger<Worker> logger) : BackgroundService
    {


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var customer = DataGenerator.GetCustomer();
            var customerId = await customersModule.CreateCustomerAsync(customer);

            logger.LogInformation("Created customer with ID: {CustomerId}", customerId);

            var order = DataGenerator.GetOrder(customerId);
            var orderId = await ordersModule.CreateOrderAsync(order);

            logger.LogInformation("Created order with ID: {OrderId} for customer ID: {CustomerId}", orderId, customerId);            


        }
    }


    public static class DataGenerator
    {
        public static CreateCustomerRequest GetCustomer()
        {
            var addressFake = new Faker<AddressDto>()
                .RuleFor(a => a.Street, f => f.Address.StreetAddress())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.State, f => f.Address.State())
                .RuleFor(a => a.ZipCode, f => f.Address.ZipCode())
                .RuleFor(a => a.Country, f => f.Address.Country());



            var customerFake = new Faker<CreateCustomerRequest>()
                 .RuleFor(c => c.Email, f => f.Internet.Email())
                 .RuleFor(c => c.LastName, f => f.Name.LastName())
                 .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                 .RuleFor(c => c.BillingAddress, addressFake)
                 .RuleFor(c => c.ShippingAddress, addressFake)
                 .RuleFor(c => c.BillingAndShippingAddressIsTheSame, f => f.PickRandom<bool>(true, false));

            return customerFake.Generate();
        }

        public static CreateOrderRequest GetOrder(Guid customerId)
        {
            var lineItemsFake = new Faker<OrderLineItemDto>()
                .RuleFor(li => li.ProductId, f => f.Random.Guid())
                .RuleFor(li => li.Quantity, f => f.Random.Int(1, 10))
                .RuleFor(li => li.UnitPrice, f => f.Finance.Amount(5, 100));

            var lineItems = lineItemsFake.Generate(3);

            var orderFake = new Faker<CreateOrderRequest>()
                .RuleFor(o => o.CustomerId, customerId)
                .RuleFor(o => o.Items, lineItems);            

            return orderFake.Generate();
        }

        
    }
}
