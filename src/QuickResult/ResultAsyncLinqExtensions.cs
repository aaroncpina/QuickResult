using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Provides LINQ query syntax support for asynchronous and mixed sync/async
/// <see cref="Result{T}"/> flows.
/// </summary>
/// <remarks>
/// Supports query expressions where either source, bind step, or both are asynchronous.
/// </remarks>
/// <example>
/// <code>
/// static Task&lt;Result&lt;int&gt;&gt; GetAsync(int n) =>
///     Task.FromResult(Result&lt;int&gt;.Success(n));
///
/// var query =
///     from a in GetAsync(4)
///     from b in GetAsync(5)
///     select a * b;
///
/// var result = await query;
/// // Success(20)
/// </code>
/// </example>
public static class ResultAsyncLinqExtensions
{
    /// <summary>
    /// Projects the success value of an asynchronous result into a new success value.
    /// </summary>
    /// <typeparam name="TIn">Type of the source success value.</typeparam>
    /// <typeparam name="TOut">Type of the projected success value.</typeparam>
    /// <param name="source">Asynchronous source result.</param>
    /// <param name="selector">Projection function.</param>
    /// <returns>
    /// A task that resolves to a projected successful result, or a propagated failure.
    /// </returns>
    public static async Task<Result<TOut>> Select<TIn, TOut>(
        this Task<Result<TIn>> source, Func<TIn, TOut> selector)
    {
        var src = await source.ConfigureAwait(false);
        return src.IsFailure
            ? Result<TOut>.Failure(src.Error)
            : Result<TOut>.Success(selector(src.Value));
    }

    /// <summary>
    /// Binds a synchronous source result to an asynchronous result and projects both values.
    /// </summary>
    /// <typeparam name="TSource">Type of the source success value.</typeparam>
    /// <typeparam name="TBind">Type of the bound success value.</typeparam>
    /// <typeparam name="TResult">Type of the final projected success value.</typeparam>
    /// <param name="source">Synchronous source result.</param>
    /// <param name="binder">Asynchronous bind function.</param>
    /// <param name="projector">Function that combines source and bound values.</param>
    /// <returns>
    /// A task that resolves to a successful projected result when both steps succeed;
    /// otherwise the first encountered failure.
    /// </returns>
    public static async Task<Result<TResult>> SelectMany<TSource, TBind, TResult>(
        this Result<TSource> source, Func<TSource, Task<Result<TBind>>> binder,
        Func<TSource, TBind, TResult> projector)
    {
        if (source.IsFailure) return Result<TResult>.Failure(source.Error);
        var sourceValue = source.Value;
        var bound = await binder(sourceValue).ConfigureAwait(false);
        return bound.IsFailure
            ? Result<TResult>.Failure(bound.Error)
            : Result<TResult>.Success(projector(sourceValue, bound.Value));
    }

    /// <summary>
    /// Binds an asynchronous source result to a synchronous result and combines both successful values.
    /// </summary>
    /// <typeparam name="TSource">The source success type.</typeparam>
    /// <typeparam name="TBind">The bound success type.</typeparam>
    /// <typeparam name="TResult">The final projected success type.</typeparam>
    /// <param name="source">The asynchronous source result.</param>
    /// <param name="binder">Synchronous binder function.</param>
    /// <param name="projector">Function that combines source and bound values.</param>
    /// <returns>A task that resolves to a combined successful result or the first encountered failure.</returns>
    public static async Task<Result<TResult>> SelectMany<TSource, TBind, TResult>(
        this Task<Result<TSource>> source, Func<TSource, Result<TBind>> binder,
        Func<TSource, TBind, TResult> projector)
    {
        var src = await source.ConfigureAwait(false);
        if (src.IsFailure) return Result<TResult>.Failure(src.Error);
        var sourceValue = src.Value;
        var bound = binder(sourceValue);
        return bound.IsFailure
            ? Result<TResult>.Failure(bound.Error)
            : Result<TResult>.Success(projector(sourceValue, bound.Value));
    }

    /// <summary>
    /// Binds an asynchronous source result to an asynchronous result and combines both successful values.
    /// </summary>
    /// <typeparam name="TSource">The source success type.</typeparam>
    /// <typeparam name="TBind">The bound success type.</typeparam>
    /// <typeparam name="TResult">The final projected success type.</typeparam>
    /// <param name="source">The asynchronous source result.</param>
    /// <param name="binder">Asynchronous binder function.</param>
    /// <param name="projector">Function that combines source and bound values.</param>
    /// <returns>A task that resolves to a combined successful result or the first encountered failure.</returns>
    public static async Task<Result<TResult>> SelectMany<TSource, TBind, TResult>(
        this Task<Result<TSource>> source, Func<TSource, Task<Result<TBind>>> binder,
        Func<TSource, TBind, TResult> projector)
    {
        var src = await source.ConfigureAwait(false);
        if (src.IsFailure) return Result<TResult>.Failure(src.Error);
        var sourceValue = src.Value;
        var bound = await binder(sourceValue).ConfigureAwait(false);
        return bound.IsFailure
            ? Result<TResult>.Failure(bound.Error)
            : Result<TResult>.Success(projector(sourceValue, bound.Value));
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> containing a <see cref="Task{TResult}"/> into
    /// a task of <see cref="Result{T}"/>, propagating both existing failures and thrown exceptions.
    /// </summary>
    /// <typeparam name="T">Type of the eventual success value.</typeparam>
    /// <param name="source">Source result that may contain an asynchronous success value.</param>
    /// <returns>
    /// A task resolving to a successful result when both layers succeed;
    /// otherwise a failure result with the propagated or exception message.
    /// </returns>
    public static async Task<Result<T>> Transpose<T>(this Result<Task<T>> source)
    {
        if (source.IsFailure) return Result.Failure<T>(source.Error);
        try
        {
            var value = await source.Value.ConfigureAwait(false);
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex.Message)
                ? ex.GetType().Name : ex.Message;
            return Result.Failure<T>(message);
        }
    }
}