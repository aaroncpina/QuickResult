using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Provides boolean guard extension methods for <see cref="Result{T}"/> workflows.
/// </summary>
/// <remarks>
/// These helpers are intentionally focused on <see cref="bool"/> results to keep the API
/// explicit and easy to understand.
/// </remarks>
public static class ResultGuardExtensions
{
    /// <summary>
    /// Returns a failure with <paramref name="error"/> when the successful boolean value is <see langword="true"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <param name="source">Source boolean result.</param>
    /// <param name="error">Failure message used when the value is <see langword="true"/>.</param>
    /// <returns>
    /// A failed result when the successful value is true; otherwise the original successful result.
    /// If <paramref name="source"/> is already failed, that failure is propagated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static Result<bool> FailIfTrue(this Result<bool> source, string error)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.IsFailure) return Result.Failure<bool>(source.Error);
        return source.Value ? Result.Failure<bool>(error) : source;
    }

    /// <summary>
    /// Asynchronously returns a failure with <paramref name="error"/> when the successful boolean value is <see langword="true"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <param name="source">Asynchronous source boolean result.</param>
    /// <param name="error">Failure message used when the value is <see langword="true"/>.</param>
    /// <returns>
    /// A task that resolves to a failed result when the successful value is true; otherwise the original successful result.
    /// If the source result is already failed, that failure is propagated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<Result<bool>> FailIfTrue(this Task<Result<bool>> source, string error)
    {
        ArgumentNullException.ThrowIfNull(source);
        var result = await source.ConfigureAwait(false);
        return result.FailIfTrue(error);
    }

    /// <summary>
    /// Returns a failure with <paramref name="error"/> when the successful boolean value is <see langword="false"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <param name="source">Source boolean result.</param>
    /// <param name="error">Failure message used when the value is <see langword="false"/>.</param>
    /// <returns>
    /// A failed result when the successful value is false; otherwise the original successful result.
    /// If <paramref name="source"/> is already failed, that failure is propagated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static Result<bool> FailIfFalse(this Result<bool> source, string error)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.IsFailure) return Result.Failure<bool>(source.Error);
        return source.Value ? source : Result.Failure<bool>(error);
    }

    /// <summary>
    /// Asynchronously returns a failure with <paramref name="error"/> when the successful boolean value is <see langword="false"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <param name="source">Asynchronous source boolean result.</param>
    /// <param name="error">Failure message used when the value is <see langword="false"/>.</param>
    /// <returns>
    /// A task that resolves to a failed result when the successful value is false; otherwise the original successful result.
    /// If the source result is already failed, that failure is propagated.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<Result<bool>> FailIfFalse(this Task<Result<bool>> source, string error)
    {
        ArgumentNullException.ThrowIfNull(source);
        var result = await source.ConfigureAwait(false);
        return result.FailIfFalse(error);
    }
}
