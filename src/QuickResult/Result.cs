using System;
using System.Threading.Tasks;

namespace QuickResult;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value
/// of type <typeparamref name="T"/> or fail with an error message.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
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
    public static Result<T> Failure(string error) => new(error);

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
    /// Projects the result into a single asynchronous value by executing one of two asynchronous delegates
    /// based on whether the result is successful or failed.
    /// </summary>
    /// <typeparam name="TResult">The return type produced by either delegate.</typeparam>
    /// <param name="onSuccess">Asynchronous delegate executed when the result is successful.</param>
    /// <param name="onFailure">Asynchronous delegate executed when the result is failed.</param>
    /// <returns>A task that resolves to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    /// <summary>
    /// Projects the result into a single asynchronous value by executing an asynchronous success delegate
    /// or a synchronous failure delegate.
    /// </summary>
    /// <typeparam name="TResult">The return type produced by either delegate.</typeparam>
    /// <param name="onSuccess">Asynchronous delegate executed when the result is successful.</param>
    /// <param name="onFailure">Delegate executed when the result is failed.</param>
    /// <returns>A task that resolves to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(Value) : Task.FromResult(onFailure(Error));
    }

    /// <summary>
    /// Projects the result into a single asynchronous value by executing a synchronous success delegate
    /// or an asynchronous failure delegate.
    /// </summary>
    /// <typeparam name="TResult">The return type produced by either delegate.</typeparam>
    /// <param name="onSuccess">Delegate executed when the result is successful.</param>
    /// <param name="onFailure">Asynchronous delegate executed when the result is failed.</param>
    /// <returns>A task that resolves to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? Task.FromResult(onSuccess(Value)) : onFailure(Error);
    }

    /// <summary>
    /// Returns the success value when successful; otherwise returns <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">Value to return when this result is failed.</param>
    /// <returns>The success value or <paramref name="fallback"/>.</returns>
    public T ValueOr(T fallback) => IsSuccess ? Value : fallback;

    /// <summary>
    /// Returns the success value when successful; otherwise returns a value produced from the failure message.
    /// </summary>
    /// <param name="fallbackFactory">Function that produces a fallback value from the failure message.</param>
    /// <returns>The success value or the value produced by <paramref name="fallbackFactory"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fallbackFactory"/> is <see langword="null"/>.
    /// </exception>
    public T ValueOr(Func<string, T> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        return IsSuccess ? Value : fallbackFactory(Error);
    }

    /// <summary>
    /// Transforms the failure message using the specified mapper while preserving successes.
    /// </summary>
    /// <param name="mapper">Mapping function applied when this result is failed.</param>
    /// <returns>
    /// This result unchanged when successful; otherwise a failed result with the mapped error message.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mapper"/> is <see langword="null"/>.
    /// </exception>
    public Result<T> MapFailure(Func<string, string> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess ? this : Result<T>.Failure(mapper(Error));
    }

    /// <summary>
    /// Transforms the success value using the specified mapper while preserving failures.
    /// </summary>
    /// <typeparam name="TOut">Type of the mapped success value.</typeparam>
    /// <param name="mapper">Mapping function applied when this result is successful.</param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> containing the mapped value when successful;
    /// otherwise a propagated failure.
    /// </returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(Value)) : Result<TOut>.Failure(Error);

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
    ///         : Result&lt;int&gt;.Failure("Not a positive integer");
    ///
    /// var parsed = Result&lt;string&gt;.Success("42").Bind(ParsePositive);
    /// </code>
    /// </example>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(Value) : Result<TOut>.Failure(Error);

    /// <summary>
    /// Returns a string representation of the current result.
    /// </summary>
    /// <returns><c>Success(value)</c> for success, or <c>Failure(error)</c> for failure.</returns>
    public override string ToString() =>
        IsSuccess ? $"Success({Value})" : $"Failure({Error})";
}

/// <summary>
/// Convenience factory methods for creating typed <see cref="Result{T}"/> instances
/// with less call-site verbosity.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the success value.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a successful result with no payload.
    /// </summary>
    /// <returns>A successful <see cref="Result{T}"/> with <see cref="Unit"/>.</returns>
    public static Result<Unit> Success() => Result<Unit>.Success(Unit.Value);

    /// <summary>
    /// Creates a failed result containing the specified error message.
    /// </summary>
    /// <typeparam name="T">Type of the success value the failure belongs to.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);

    /// <summary>
    /// Creates a result from a nullable reference value.
    /// Returns success when <paramref name="value"/> is not null; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">Type of the success value.</typeparam>
    /// <param name="value">Nullable reference value to wrap.</param>
    /// <param name="error">Failure message when <paramref name="value"/> is null.</param>
    /// <returns>A successful or failed <see cref="Result{T}"/> depending on null state.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="error"/> is null, empty, or whitespace and <paramref name="value"/> is null.
    /// </exception>
    public static Result<T> FromNullable<T>(T? value, string error)
        where T : class =>
        value is null ? Failure<T>(error) : Success(value);

    /// <summary>
    /// Creates a result from a nullable value type.
    /// Returns success when <paramref name="value"/> has a value; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">Type of the success value.</typeparam>
    /// <param name="value">Nullable value type to wrap.</param>
    /// <param name="error">Failure message when <paramref name="value"/> has no value.</param>
    /// <returns>A successful or failed <see cref="Result{T}"/> depending on null state.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="error"/> is null, empty, or whitespace and <paramref name="value"/> has no value.
    /// </exception>
    public static Result<T> FromNullable<T>(T? value, string error)
        where T : struct =>
        value.HasValue ? Success(value.Value) : Failure<T>(error);

    /// <summary>
    /// Executes <paramref name="func"/> and wraps its outcome in a <see cref="Result{T}"/>.
    /// Returns <see cref="Result{T}.Failure(string)"/> with the exception message when an exception is thrown.
    /// </summary>
    /// <typeparam name="T">Type of the produced value.</typeparam>
    /// <param name="func">Function to execute.</param>
    /// <returns>
    /// <see cref="Result{T}.Success(T)"/> when <paramref name="func"/> succeeds;
    /// otherwise <see cref="Result{T}.Failure(string)"/> with the thrown exception message.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    public static Result<T> Try<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? ex.GetType().Name : ex.Message;
            return Failure<T>(message);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> and wraps its outcome in a <see cref="Result{T}"/> of <see cref="Unit"/>.
    /// Returns <see cref="Result{T}.Failure(string)"/> with the exception message when an exception is thrown.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <returns>
    /// <see cref="Success()"/> when <paramref name="action"/> succeeds;
    /// otherwise <see cref="Failure{T}(string)"/> for <see cref="Unit"/> with the thrown exception message.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static Result<Unit> Try(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? ex.GetType().Name : ex.Message;
            return Failure<Unit>(message);
        }
    }

    /// <summary>
    /// Executes an asynchronous function and wraps its outcome in a <see cref="Result{T}"/>.
    /// Returns <see cref="Result{T}.Failure(string)"/> with the thrown exception message when an exception is thrown.
    /// </summary>
    /// <typeparam name="T">Type of the produced value.</typeparam>
    /// <param name="func">Asynchronous function to execute.</param>
    /// <returns>
    /// A task that resolves to <see cref="Result{T}.Success(T)"/> when <paramref name="func"/> succeeds;
    /// otherwise <see cref="Result{T}.Failure(string)"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            return Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? ex.GetType().Name : ex.Message;
            return Failure<T>(message);
        }
    }

    /// <summary>
    /// Executes an asynchronous operation and wraps its outcome in a <see cref="Result{T}"/> of <see cref="Unit"/>.
    /// Returns <see cref="Result{T}.Failure(string)"/> with the thrown exception message when an exception is thrown.
    /// </summary>
    /// <param name="func">Asynchronous operation to execute.</param>
    /// <returns>
    /// A task that resolves to <see cref="Success()"/> when <paramref name="func"/> succeeds;
    /// otherwise <see cref="Failure{T}(string)"/> for <see cref="Unit"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    public static async Task<Result<Unit>> TryAsync(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        try
        {
            await func().ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? ex.GetType().Name : ex.Message;
            return Failure<Unit>(message);
        }
    }
}
