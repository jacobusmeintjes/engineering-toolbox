using Contracts.Requests;
using Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Domain;
using PaymentService.Repositories;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentsController(
        IPaymentGateway gateway,
        IPaymentRepository repo,
        ILogger<PaymentsController> logger) : ControllerBase
    {
        [HttpPost("authorise")]
        [ProducesResponseType(typeof(AuthorisePaymentResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthorisePaymentResponse>> Authorise(
            AuthorisePaymentRequest request, CancellationToken ct)
        {
            logger.LogInformation(
             "REST — authorising payment for order {OrderId}", request.OrderId);

            var result = await gateway.AuthoriseAsync(
                request.PaymentMethodToken, request.Amount, ct);

            var record = result.Success
                ? PaymentRecord.CreateAuthorised(
                    request.OrderId, request.Amount, result.TransactionId!)
                : PaymentRecord.CreateFailed(
                    request.OrderId, request.Amount, result.FailureReason!);

            await repo.SaveAsync(record, ct);

            return Ok(new AuthorisePaymentResponse(
                result.Success,
                result.TransactionId,
                result.FailureReason));
        }

        [HttpPost("void")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Void(
            VoidPaymentRequest request, CancellationToken ct)
        {
            logger.LogInformation(
           "REST — voiding transaction {TransactionId}", request.TransactionId);

            var record = await repo.GetByTransactionIdAsync(request.TransactionId, ct);

            if (record is null)
                return NotFound($"Transaction {request.TransactionId} not found");

            await gateway.VoidAsync(request.TransactionId, ct);

            record.Void();
            await repo.UpdateAsync(record, ct);

            return NoContent();
        }

        // Shared read endpoint — useful for checking payment status in either mode
        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(PaymentStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentStatusResponse>> GetByOrderId(
            Guid orderId, CancellationToken ct)
        {
            var record = await repo.GetByOrderIdAsync(orderId, ct);

            if (record is null)
                return NotFound($"No payment found for order {orderId}");

            return Ok(new PaymentStatusResponse(
                record.OrderId,
                record.Status.ToString(),
                record.TransactionId,
                record.Amount,
                record.FailureReason,
                record.CreatedAt));
        }
    }
}
