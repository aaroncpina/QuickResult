namespace QuickResult;

/// <summary>
/// Represents an error associated with a failed <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
/// Implement this interface to create custom, domain-specific error types
/// (e.g. HTTP errors, validation errors) that integrate seamlessly with
/// <see cref="Result{T}"/> via pattern matching (<c>is</c>, <c>as</c>, <c>switch</c>).
/// </remarks>
public interface IError
{
    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    string Message { get; }
}
