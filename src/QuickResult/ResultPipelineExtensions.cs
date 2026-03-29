using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Composable pipeline operators for Result workflows.
/// </summary>
public static class ResultPipelineExtensions
{
    /// <summary>
    /// Executes the pipeline and captures any thrown exception as a failure.
    /// </summary>
    /// <typeparam name="T">Type of the produced value.</typeparam>
    /// <param name="pipeline">The deferred pipeline to execute.</param>
    /// <returns>
    /// A task that resolves to <see cref="Result{T}"/> containing the produced value on success,
    /// or a failure with the exception message when an exception is thrown.
    /// </returns>
    public static async Task<Result<T>> Try<T>(this ResultPipeline<T> pipeline)
    {
        try
        {
            return Result.Success(await pipeline.ExecuteAsync().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
            return Result.Failure<T>(message);
        }
    }

    /// <summary>
    /// Converts a successful nullable reference result into a failure when the value is <see langword="null"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the non-null success value.</typeparam>
    /// <param name="source">Asynchronous source result containing a nullable reference value.</param>
    /// <param name="error">Failure message used when the value is <see langword="null"/>.</param>
    /// <returns>
    /// A task that resolves to a successful result with a non-null value,
    /// or a failure if the value was null or the source was already failed.
    /// </returns>
    public static async Task<Result<T>> WhenNull<T>(this Task<Result<T?>> source, string error)
        where T : class
    {
        var result = await source.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<T>(result.Error);
        return result.Value is null ? Result.Failure<T>(error) : Result.Success(result.Value);
    }

    /// <summary>
    /// Converts a successful nullable value-type result into a failure when the value is <see langword="null"/>.
    /// Existing failures are propagated unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the underlying value type.</typeparam>
    /// <param name="source">Asynchronous source result containing a nullable value type.</param>
    /// <param name="error">Failure message used when the value is <see langword="null"/>.</param>
    /// <returns>
    /// A task that resolves to a successful result with the unwrapped value,
    /// or a failure if the value had no value or the source was already failed.
    /// </returns>
    public static async Task<Result<T>> WhenNull<T>(this Task<Result<T?>> source, string error)
        where T : struct
    {
        var result = await source.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<T>(result.Error);
        return result.Value.HasValue ? Result.Success(result.Value.Value) : Result.Failure<T>(error);
    }
}
