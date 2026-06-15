using Contracts.Domain;
using Contracts.Requests;
using Contracts.Responses;
using InventoryService.Domain;
using InventoryService.Repositories;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    // Controllers/InventoryController.cs
    [ApiController]
    [Route("inventory")]
    [Produces("application/json")]
    public class InventoryController(
        IInventoryService inventoryService,
        IInventoryRepository repo,
        ILogger<InventoryController> logger) : ControllerBase
    {
        [HttpPut("reserve")]
        [ProducesResponseType(typeof(ReserveStockResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReserveStockResponse>> Reserve(
            ReserveStockRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "Reserving stock for order {OrderId}", request.OrderId);

            var result = await inventoryService.ReserveAsync(request, ct);
            return Ok(result);
        }

        [HttpPut("release")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Release(
            ReleaseStockRequest request, CancellationToken ct)
        {
            logger.LogInformation(
                "Releasing stock for order {OrderId}", request.OrderId);

            await inventoryService.ReleaseAsync(request, ct);
            return NoContent();
        }

        [HttpGet("{sku}")]
        [ProducesResponseType(typeof(StockLevelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockLevelResponse>> GetStockLevel(
            string sku, CancellationToken ct)
        {
            var item = await repo.GetBySkuAsync(sku, ct);

            if (item is null)
                return NotFound($"SKU {sku} not found");

            return Ok(new StockLevelResponse(
                item.Sku,
                item.ProductName,
                item.TotalQuantity,
                item.ReservedQuantity,
                item.AvailableQuantity));
        }

        [HttpPost("stock")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateStockItem(
            CreateStockItemRequest request, CancellationToken ct)
        {
            var item = StockItem.Create(request.Sku, request.ProductName, request.Quantity);
            await repo.SaveStockItemAsync(item, ct);

            return CreatedAtAction(
                nameof(GetStockLevel),
                new { sku = item.Sku },
                null);
        }
    }
}
