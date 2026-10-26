using System.ComponentModel.DataAnnotations;

namespace TestMetrics.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1886, 2100)]
    public int Year { get; set; }

    public string? Vin { get; set; }
}
