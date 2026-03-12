namespace QuickResult;

/// <summary>
/// Represents a void-like value for operations that succeed without producing data.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// The single <see cref="Unit"/> value.
    /// </summary>
    public static readonly Unit Value = new();
}
