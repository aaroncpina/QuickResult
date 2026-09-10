using System;
using System.Runtime.CompilerServices;

namespace QuickResult;

/// <summary>
/// Argument validation helpers that work on every target framework.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> when <paramref name="argument"/> is null.
    /// Equivalent to <c>ArgumentNullException.ThrowIfNull</c>, which is unavailable on .NET Standard 2.0.
    /// </summary>
    public static void NotNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }
}
