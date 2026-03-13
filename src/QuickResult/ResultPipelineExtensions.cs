using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Composable pipeline operators for Result workflows.
/// </summary>
public static class ResultPipelineExtensions
{
    /// <summary>
    /// Executes pipeline and captures thrown exceptions as failure.
    /// </summary>
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
    /// Converts a successful nullable reference value to failure when null.
    /// </summary>
    public static async Task<Result<T>> WhenNull<T>(this Task<Result<T?>> source, string error)
        where T : class
    {
        var result = await source.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<T>(result.Error);
        return result.Value is null ? Result.Failure<T>(error) : Result.Success(result.Value);
    }

    /// <summary>
    /// Converts a successful nullable value type to failure when null.
    /// </summary>
    public static async Task<Result<T>> WhenNull<T>(this Task<Result<T?>> source, string error)
        where T : struct
    {
        var result = await source.ConfigureAwait(false);
        if (result.IsFailure) return Result.Failure<T>(result.Error);
        return result.Value.HasValue ? Result.Success(result.Value.Value) : Result.Failure<T>(error);
    }
}
