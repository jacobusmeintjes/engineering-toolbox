using Contracts.Requests;
using Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Repositories;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    // Controllers/NotificationsController.cs
    [ApiController]
    [Route("notifications")]
    [Produces("application/json")]
    public class NotificationsController(
        INotificationService notificationService,
        INotificationRepository repo,
        ILogger<NotificationsController> logger) : ControllerBase
    {
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Send(
            SendNotificationRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "Received {EventType} notification request for order {OrderId}",
                request.EventType, request.OrderId);

            await notificationService.SendAsync(request, ct);
            return Accepted();
        }

        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationLogResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<NotificationLogResponse>>> GetByOrder(
            Guid orderId, CancellationToken ct)
        {
            var records = await repo.GetByOrderIdAsync(orderId, ct);
            return Ok(records.Select(r => r.ToResponse()).ToList());
        }

        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationLogResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<NotificationLogResponse>>> GetByCustomer(
            Guid customerId, CancellationToken ct)
        {
            var records = await repo.GetByCustomerIdAsync(customerId, ct);
            return Ok(records.Select(r => r.ToResponse()).ToList());
        }
    }
}
