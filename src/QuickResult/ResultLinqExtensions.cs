using System;

namespace QuickResult;

/// <summary>
/// Provides LINQ query syntax support for synchronous <see cref="Result{T}"/> values.
/// </summary>
/// <remarks>
/// These extension methods back C# query expressions using <c>from</c> / <c>select</c>
/// over <see cref="Result{T}"/>.
/// </remarks>
/// <example>
/// <code>
/// var query =
///     from a in Result&lt;int&gt;.Success(10)
///     from b in Result&lt;int&gt;.Success(5)
///     select a + b;
/// // Success(15)
/// </code>
/// </example>
public static class ResultLinqExtensions
{
    /// <summary>
    /// Projects the success value of a result into a new success value.
    /// </summary>
    /// <typeparam name="TIn">Type of the source success value.</typeparam>
    /// <typeparam name="TOut">Type of the projected success value.</typeparam>
    /// <param name="source">Source result.</param>
    /// <param name="selector">Projection function.</param>
    /// <returns>
    /// A projected successful result, or a propagated failure from <paramref name="source"/>.
    /// </returns>
    public static Result<TOut> Select<TIn, TOut>(
        this Result<TIn> source, Func<TIn, TOut> selector) =>
        source.Map(selector);

    /// <summary>
    /// Binds a successful source value to another result and projects both values.
    /// </summary>
    /// <typeparam name="TSource">Type of the source success value.</typeparam>
    /// <typeparam name="TBind">Type of the bound success value.</typeparam>
    /// <typeparam name="TResult">Type of the final projected success value.</typeparam>
    /// <param name="source">Source result.</param>
    /// <param name="binder">Function that produces the next result.</param>
    /// <param name="projector">Function that combines source and bound values.</param>
    /// <returns>
    /// A successful projected result when both steps succeed; otherwise the first encountered failure.
    /// </returns>
    public static Result<TResult> SelectMany<TSource, TBind, TResult>(
        this Result<TSource> source,
        Func<TSource, Result<TBind>> binder, Func<TSource, TBind, TResult> projector)
    {
        if (source.IsFailure) return Result<TResult>.Failure(source.Error);
        var sourceValue = source.Value;
        var bound = binder(sourceValue);
        return bound.IsFailure
            ? Result<TResult>.Failure(bound.Error)
            : Result<TResult>.Success(projector(sourceValue, bound.Value));
    }
}
