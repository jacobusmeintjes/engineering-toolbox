using System.Collections.Concurrent;
using TestMetrics.Models;

namespace TestMetrics.Services;

public class VehicleStore
{
    private readonly ConcurrentDictionary<int, Vehicle> _vehicles = new();
    private int _nextId;

    public IEnumerable<Vehicle> GetAll() => _vehicles.Values.OrderBy(v => v.Id);

    public Vehicle? Get(int id) => _vehicles.GetValueOrDefault(id);

    public Vehicle Add(Vehicle vehicle)
    {
        vehicle.Id = Interlocked.Increment(ref _nextId);
        _vehicles[vehicle.Id] = vehicle;
        return vehicle;
    }

    public bool Update(int id, Vehicle vehicle)
    {
        if (!_vehicles.ContainsKey(id)) return false;
        vehicle.Id = id;
        _vehicles[id] = vehicle;
        return true;
    }

    public bool Delete(int id) => _vehicles.TryRemove(id, out _);
}
