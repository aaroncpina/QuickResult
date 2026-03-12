using System;

namespace QuickResult;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value
/// of type <typeparamref name="T"/> or fail with an error message.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
/// <remarks>
/// This type is intended for explicit error handling without relying on exceptions
/// for expected control flow.
/// </remarks>
/// <example>
/// <code>
/// var result = Result&lt;int&gt;.Success(5)
///     .Map(x => x * 2);
/// // Success(10)
/// </code>
/// </example>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    /// <summary>
    /// Gets a value indicating whether the result represents success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a failure result.
    /// </exception>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value when result is failure.");

    /// <summary>
    /// Gets the failure error message.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a success result.
    /// </exception>
    public string Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Cannot access Error when result is success.");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error must not be null or whitespace.", nameof(error));
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result containing the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="error"/> is null, empty, or whitespace.
    /// </exception>
    public static Result<T> Fail(string error) => new(error);

    /// <summary>
    /// Returns <paramref name="left"/> when it is successful; otherwise returns <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The primary result.</param>
    /// <param name="right">The fallback result.</param>
    /// <returns>
    /// <paramref name="left"/> if it is successful; otherwise <paramref name="right"/>.
    /// </returns>
    /// <remarks>
    /// This operator is useful for fallback strategies where an alternate result should be used
    /// when the primary result fails.
    /// </remarks>
    public static Result<T> operator |(Result<T> left, Result<T> right) =>
        left.IsSuccess ? left : right;

    /// <summary>
    /// Projects the result into a single value by executing one of two delegates
    /// based on whether the result is successful or failed.
    /// </summary>
    /// <typeparam name="TResult">The return type produced by either delegate.</typeparam>
    /// <param name="onSuccess">Delegate executed when the result is successful.</param>
    /// <param name="onFailure">Delegate executed when the result is failed.</param>
    /// <returns>The value returned by the executed delegate.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Transforms the success value using the specified mapper while preserving failures.
    /// </summary>
    /// <typeparam name="TOut">Type of the mapped success value.</typeparam>
    /// <param name="mapper">Mapping function applied when this result is successful.</param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> containing the mapped value when successful;
    /// otherwise a propagated failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var length = Result&lt;string&gt;.Success("hello")
    ///     .Map(s => s.Length);
    /// // Success(5)
    /// </code>
    /// </example>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(Value)) : Result<TOut>.Fail(Error);

    /// <summary>
    /// Chains a result-producing operation onto a successful result while preserving failures.
    /// </summary>
    /// <typeparam name="TOut">Type of the chained success value.</typeparam>
    /// <param name="binder">Function that returns the next result.</param>
    /// <returns>
    /// The result returned by <paramref name="binder"/> when successful;
    /// otherwise a propagated failure.
    /// </returns>
    /// <example>
    /// <code>
    /// Result&lt;int&gt; ParsePositive(string s) =>
    ///     int.TryParse(s, out var n) &amp;&amp; n &gt; 0
    ///         ? Result&lt;int&gt;.Success(n)
    ///         : Result&lt;int&gt;.Fail("Not a positive integer");
    ///
    /// var parsed = Result&lt;string&gt;.Success("42").Bind(ParsePositive);
    /// </code>
    /// </example>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(Value) : Result<TOut>.Fail(Error);

    /// <summary>
    /// Returns a string representation of the current result.
    /// </summary>
    /// <returns><c>Success(value)</c> for success, or <c>Failure(error)</c> for failure.</returns>
    public override string ToString() =>
        IsSuccess ? $"Success({Value})" : $"Failure({Error})";
}