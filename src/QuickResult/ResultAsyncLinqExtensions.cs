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
            ? Result<TOut>.Fail(src.Error)
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
        if (source.IsFailure) return Result<TResult>.Fail(source.Error);
        var sourceValue = source.Value;
        var bound = await binder(sourceValue).ConfigureAwait(false);
        return bound.IsFailure
            ? Result<TResult>.Fail(bound.Error)
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
        if (src.IsFailure) return Result<TResult>.Fail(src.Error);
        var sourceValue = src.Value;
        var bound = binder(sourceValue);
        return bound.IsFailure
            ? Result<TResult>.Fail(bound.Error)
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
        if (src.IsFailure) return Result<TResult>.Fail(src.Error);
        var sourceValue = src.Value;
        var bound = await binder(sourceValue).ConfigureAwait(false);
        return bound.IsFailure
            ? Result<TResult>.Fail(bound.Error)
            : Result<TResult>.Success(projector(sourceValue, bound.Value));
    }
}