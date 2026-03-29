using System;

namespace QuickResult;

/// <summary>
/// Default <see cref="IError"/> implementation that wraps a simple string message.
/// </summary>
/// <remarks>
/// Provides implicit conversions to and from <see cref="string"/> so that
/// plain string error messages work transparently wherever an <see cref="IError"/> is expected.
/// </remarks>
public sealed class Error : IError, IEquatable<Error>
{
    /// <inheritdoc />
    public string Message { get; }

    /// <summary>
    /// Initialises a new <see cref="Error"/> with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="message"/> is null, empty, or whitespace.
    /// </exception>
    public Error(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message must not be null or whitespace.", nameof(message));
        Message = message;
    }

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="Error"/>.
    /// </summary>
    public static implicit operator Error(string message) => new(message);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to its <see cref="string"/> message.
    /// </summary>
    public static implicit operator string(Error error) => error.Message;

    /// <inheritdoc />
    public override string ToString() => Message;

    /// <inheritdoc />
    public bool Equals(Error? other) => other is not null && Message == other.Message;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Error other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Message.GetHashCode();
}
