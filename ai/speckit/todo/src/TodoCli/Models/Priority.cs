namespace TodoCli.Models;

/// <summary>
/// Represents task importance levels for sorting and visual highlighting.
/// </summary>
public enum Priority
{
    /// <summary>
    /// Low priority task (value: 0)
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority task (value: 1, default)
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority task (value: 2)
    /// </summary>
    High = 2
}
