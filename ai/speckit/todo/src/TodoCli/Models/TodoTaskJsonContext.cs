using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoCli.Models;

/// <summary>
/// JSON serialization context for TodoTask with source generators (performance optimization)
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    Converters = new[] { typeof(JsonStringEnumConverter) }
)]
[JsonSerializable(typeof(List<TodoTask>))]
[JsonSerializable(typeof(TodoTask))]
[JsonSerializable(typeof(Priority))]
public partial class TodoTaskJsonContext : JsonSerializerContext
{
}
