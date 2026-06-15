using Contracts.Domain;
using Contracts.Requests;
using Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using OrderService.Repositories;
using OrderService.Services;

namespace OrderService.Controllers
{
    // Controllers/OrdersController.cs
    [ApiController]
    [Route("[controller]")]
    public class OrdersController(
        RestOrchestrator restOrchestrator,
        EventDrivenOrderOrchestrator eventDrivenOrderOrchestrator,
        //IOrderRepository orders,
        ILogger<OrdersController> logger) : ControllerBase
    {
        [HttpPost("rest")]
        public async Task<ActionResult<OrderResponse>> PlaceOrder(
            PlaceOrderRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "Placing order for customer {CustomerId} with {ItemCount} items",
                request.CustomerId, request.Items.Count);
             
            var response = await restOrchestrator.PlaceOrderAsync(request, ct);

            return Ok(new { orderId = response.OrderId });
            //return CreatedAtAction(
            //    nameof(GetById),
            //    new { orderId = response.OrderId },
            //    response);
        }

        [HttpPost("events")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status202Accepted)]
        public async Task<ActionResult<OrderResponse>> PlaceOrderEvent(
            PlaceOrderRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "EVENT — placing order for customer {CustomerId}", request.CustomerId);

            var response = await eventDrivenOrderOrchestrator.PlaceOrderAsync(request, ct);

            return Ok(new { orderId = response.OrderId });
            //return AcceptedAtAction(
            //    nameof(GetById),
            //    new { orderId = response.OrderId },
            //    response);
        }

        //[HttpGet("{orderId:guid}")]
        //public async Task<ActionResult<OrderResponse>> GetById(
        //    Guid orderId, CancellationToken ct)
        //{
        //    var order = await orders.GetByIdAsync(orderId, ct);

        //    if (order is null)
        //        return NotFound($"Order {orderId} not found");

        //    return Ok(order.ToResponse());
        //}

        //[HttpGet("customer/{customerId:guid}")]
        //public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetByCustomer(
        //    Guid customerId, CancellationToken ct)
        //{
        //    var customerOrders = await orders.GetByCustomerIdAsync(customerId, ct);
        //    return Ok(customerOrders.Select(o => o.ToResponse()).ToList());
        //}

        //[HttpPost("{orderId:guid}/cancel")]
        //public async Task<ActionResult<OrderResponse>> Cancel(
        //    Guid orderId, CancellationToken ct)
        //{
        //    logger.LogInformation("Cancelling order {OrderId}", orderId);

        //    var order = await orders.GetByIdAsync(orderId, ct);

        //    if (order is null)
        //        return NotFound($"Order {orderId} not found");

        //    order.Transition(OrderStatus.CancelledByCustomer);
        //    await orders.UpdateAsync(order, ct);

        //    return Ok(order.ToResponse());
        //}
    }
}
