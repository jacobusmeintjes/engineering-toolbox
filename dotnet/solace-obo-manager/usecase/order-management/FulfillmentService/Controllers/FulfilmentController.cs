using Contracts.Requests;
using Contracts.Responses;
using FulfillmentService.Domain;
using FulfillmentService.Repositories;
using FulfillmentService.Services;
using Messaging.Abstractions;
using Messaging.Events.Fulfilment;
using Messaging.Topics;
using Microsoft.AspNetCore.Mvc;

namespace FulfillmentService.Controllers
{
    // Controllers/FulfilmentController.cs
    [ApiController]
    [Route("fulfilment")]
    [Produces("application/json")]
    public class FulfilmentController(
        IShipmentRepository repo,
        IWarehouseSystem warehouse,
        ICarrierService carrier,
        IEventPublisher publisher,
        ILogger<FulfilmentController> logger) : ControllerBase
    {
        [HttpPost("shipments")]
        [ProducesResponseType(typeof(CreateShipmentResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult<CreateShipmentResponse>> CreateShipment(
            CreateShipmentRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "Creating shipment for order {OrderId}", request.OrderId);

            var shipment = Shipment.Create(
                request.OrderId,
                request.CustomerId,
                request.ShippingAddress,
                request.Items);

            await repo.SaveAsync(shipment, ct);

            // Instruct warehouse to begin picking
            await warehouse.CreatePickListAsync(shipment.Id, shipment.LineItems, ct);
            shipment.StartPicking();
            await repo.UpdateAsync(shipment, ct);

            logger.LogInformation(
                "Shipment {ShipmentId} created for order {OrderId}",
                shipment.Id, request.OrderId);

            return CreatedAtAction(
                nameof(GetById),
                new { shipmentId = shipment.Id },
                new CreateShipmentResponse(shipment.Id.ToString(), shipment.EstimatedDelivery));
        }
         
        [HttpGet("{shipmentId:guid}")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipmentResponse>> GetById(
            Guid shipmentId, CancellationToken ct)
        {
            var shipment = await repo.GetByIdAsync(shipmentId, ct);

            if (shipment is null)
                return NotFound($"Shipment {shipmentId} not found");

            return Ok(shipment.ToResponse());
        }

        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipmentResponse>> GetByOrderId(
            Guid orderId, CancellationToken ct)
        {
            var shipment = await repo.GetByOrderIdAsync(orderId, ct);

            if (shipment is null)
                return NotFound($"No shipment found for order {orderId}");

            return Ok(shipment.ToResponse());
        }

        [HttpPost("{shipmentId:guid}/pack")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ShipmentResponse>> MarkPacked(
            Guid shipmentId, CancellationToken ct)
        {
            var shipment = await repo.GetByIdAsync(shipmentId, ct);

            if (shipment is null)
                return NotFound($"Shipment {shipmentId} not found");

            shipment.MarkPacked();
            await repo.UpdateAsync(shipment, ct);

            return Ok(shipment.ToResponse());
        }        


        [HttpPost("{shipmentId:guid}/ship")]
        public async Task<ActionResult<ShipmentResponse>> MarkShipped(
    Guid shipmentId, CancellationToken ct)
        {
            var shipment = await repo.GetByIdAsync(shipmentId, ct);
            if (shipment is null) return NotFound($"Shipment {shipmentId} not found");

            var booking = await carrier.BookCollectionAsync(shipment, ct);
            shipment.MarkShipped(booking.TrackingNumber, booking.CarrierCode);
            await repo.UpdateAsync(shipment, ct);

            await publisher.PublishAsync(
                new ShipmentShipped
                {
                    ShipmentId = shipment.Id,
                    OrderId = shipment.OrderId,
                    CustomerId = shipment.CustomerId,
                    TrackingNumber = booking.TrackingNumber,
                    CarrierCode = booking.CarrierCode,
                    EstimatedDelivery = booking.EstimatedDelivery
                },
                Topics.Fulfilment.ShipmentShipped, ct);

            return Ok(shipment.ToResponse());
        }

        [HttpPost("{shipmentId:guid}/deliver")]
        public async Task<ActionResult<ShipmentResponse>> MarkDelivered(
            Guid shipmentId, CancellationToken ct)
        {
            var shipment = await repo.GetByIdAsync(shipmentId, ct);
            if (shipment is null) return NotFound($"Shipment {shipmentId} not found");

            shipment.MarkDelivered();
            await repo.UpdateAsync(shipment, ct);

            await publisher.PublishAsync(
                new ShipmentDelivered
                {
                    ShipmentId = shipment.Id,
                    OrderId = shipment.OrderId,
                    CustomerId = shipment.CustomerId,
                    DeliveredAt = shipment.DeliveredAt!.Value
                },
                Topics.Fulfilment.ShipmentDelivered, ct);

            return Ok(shipment.ToResponse());
        }
    }
}
