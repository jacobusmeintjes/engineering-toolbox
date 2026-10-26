using Microsoft.AspNetCore.Mvc;
using TestMetrics.Models;
using TestMetrics.Services;

namespace TestMetrics.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController(VehicleStore store, ILogger<VehiclesController> logger) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Vehicle>> GetAll()
    {
        logger.LogInformation("Listing vehicles");
        var vehicles = store.GetAll().ToList();
        logger.LogInformation("Listed {VehicleCount} vehicles", vehicles.Count);
        return Ok(vehicles);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Vehicle> Get(int id)
    {
        logger.LogInformation("Fetching vehicle {VehicleId}", id);
        if (store.Get(id) is not { } vehicle)
        {
            logger.LogWarning("Vehicle {VehicleId} was not found", id);
            return NotFound();
        }

        logger.LogInformation("Found vehicle {VehicleId}", id);
        return vehicle;
    }

    [HttpPost]
    public ActionResult<Vehicle> Create(Vehicle vehicle)
    {
        logger.LogInformation("Creating vehicle {Make} {Model} for year {Year}", vehicle.Make, vehicle.Model, vehicle.Year);
        var created = store.Add(vehicle);
        logger.LogInformation("Created vehicle {VehicleId}", created.Id);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Vehicle vehicle)
    {
        logger.LogInformation("Updating vehicle {VehicleId}", id);
        if (!store.Update(id, vehicle))
        {
            logger.LogWarning("Cannot update vehicle {VehicleId}: it was not found", id);
            return NotFound();
        }

        logger.LogInformation("Updated vehicle {VehicleId}", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        logger.LogInformation("Deleting vehicle {VehicleId}", id);
        if (!store.Delete(id))
        {
            logger.LogWarning("Cannot delete vehicle {VehicleId}: it was not found", id);
            return NotFound();
        }

        logger.LogInformation("Deleted vehicle {VehicleId}", id);
        return NoContent();
    }
}
