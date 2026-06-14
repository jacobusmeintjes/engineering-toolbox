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
        public async Task<ActionResult<AuthorisePaymentResponse>> Authorise(
            AuthorisePaymentRequest request, CancellationToken ct)
        {
            logger.LogInformation("Authorising payment for order {OrderId}", request.OrderId);

            var result = await gateway.AuthoriseAsync(
                request.PaymentMethodToken, request.Amount, ct);

            var record = result.Success
                ? PaymentRecord.CreateAuthorised(request.OrderId, request.Amount, result.TransactionId!)
                : PaymentRecord.CreateFailed(request.OrderId, request.Amount, result.FailureReason!);

            await repo.SaveAsync(record, ct);

            return Ok(new AuthorisePaymentResponse(
                result.Success,
                result.TransactionId,
                result.FailureReason));
        }

        [HttpPost("void")]
        public async Task<IActionResult> Void(
            VoidPaymentRequest request, CancellationToken ct)
        {
            logger.LogInformation("Voiding transaction {TransactionId}", request.TransactionId);

            var record = await repo.GetByTransactionIdAsync(request.TransactionId, ct);

            if (record is null)
                return NotFound($"Transaction {request.TransactionId} not found");

            await gateway.VoidAsync(request.TransactionId, ct);

            record.Void();
            await repo.UpdateAsync(record, ct);

            return NoContent();
        }
    }
}
