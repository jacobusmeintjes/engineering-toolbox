// MyApp.Modules.Orders.Public/OrderDto.cs
namespace MyApp.Modules.Orders.Internal.Models;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
