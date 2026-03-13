using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Provides branch/projection helpers for <see cref="Result{T}"/> and <see cref="Task{TResult}"/>-wrapped results.
/// </summary>
/// <remarks>
/// These methods centralize success/failure branching and support both synchronous and asynchronous handlers.
/// </remarks>
public static class ResultMatchExtensions
{
    /// <summary>
    /// Projects a result into a single value by invoking one of two synchronous delegates.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Source result to branch on.</param>
    /// <param name="onSuccess">Delegate invoked when <paramref name="source"/> is successful.</param>
    /// <param name="onFailure">Delegate invoked when <paramref name="source"/> is failed.</param>
    /// <returns>The value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static TResult Match<T, TResult>(
        this Result<T> source,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return source.IsSuccess ? onSuccess(source.Value) : onFailure(source.Error);
    }

    /// <summary>
    /// Projects a result into a single asynchronous value by invoking one of two asynchronous delegates.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Source result to branch on.</param>
    /// <param name="onSuccess">Asynchronous delegate invoked when <paramref name="source"/> is successful.</param>
    /// <param name="onFailure">Asynchronous delegate invoked when <paramref name="source"/> is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static Task<TResult> MatchAsync<T, TResult>(
        this Result<T> source,
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return source.IsSuccess ? onSuccess(source.Value) : onFailure(source.Error);
    }

    /// <summary>
    /// Projects a result into a single asynchronous value using an asynchronous success delegate
    /// and a synchronous failure delegate.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Source result to branch on.</param>
    /// <param name="onSuccess">Asynchronous delegate invoked when <paramref name="source"/> is successful.</param>
    /// <param name="onFailure">Synchronous delegate invoked when <paramref name="source"/> is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static Task<TResult> MatchAsync<T, TResult>(
        this Result<T> source,
        Func<T, Task<TResult>> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return source.IsSuccess ? onSuccess(source.Value) : Task.FromResult(onFailure(source.Error));
    }

    /// <summary>
    /// Projects a result into a single asynchronous value using a synchronous success delegate
    /// and an asynchronous failure delegate.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Source result to branch on.</param>
    /// <param name="onSuccess">Synchronous delegate invoked when <paramref name="source"/> is successful.</param>
    /// <param name="onFailure">Asynchronous delegate invoked when <paramref name="source"/> is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static Task<TResult> MatchAsync<T, TResult>(
        this Result<T> source,
        Func<T, TResult> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return source.IsSuccess ? Task.FromResult(onSuccess(source.Value)) : onFailure(source.Error);
    }

    /// <summary>
    /// Projects an asynchronous result source into a single asynchronous value by invoking
    /// one of two asynchronous delegates.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Asynchronous source result.</param>
    /// <param name="onSuccess">Asynchronous delegate invoked when the awaited source is successful.</param>
    /// <param name="onFailure">Asynchronous delegate invoked when the awaited source is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await source.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }

    /// <summary>
    /// Projects an asynchronous result source into a single asynchronous value using an asynchronous success delegate
    /// and a synchronous failure delegate.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Asynchronous source result.</param>
    /// <param name="onSuccess">Asynchronous delegate invoked when the awaited source is successful.</param>
    /// <param name="onFailure">Synchronous delegate invoked when the awaited source is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, Task<TResult>> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await source.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }

    /// <summary>
    /// Projects an asynchronous result source into a single asynchronous value using a synchronous success delegate
    /// and an asynchronous failure delegate.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Asynchronous source result.</param>
    /// <param name="onSuccess">Synchronous delegate invoked when the awaited source is successful.</param>
    /// <param name="onFailure">Asynchronous delegate invoked when the awaited source is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, TResult> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await source.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onFailure).ConfigureAwait(false);
    }

    /// <summary>
    /// Projects an asynchronous result source into a single asynchronous value using two synchronous delegates.
    /// </summary>
    /// <typeparam name="T">Type of the success value in the source result.</typeparam>
    /// <typeparam name="TResult">Type produced by either branch delegate.</typeparam>
    /// <param name="source">Asynchronous source result.</param>
    /// <param name="onSuccess">Synchronous delegate invoked when the awaited source is successful.</param>
    /// <param name="onFailure">Synchronous delegate invoked when the awaited source is failed.</param>
    /// <returns>A task resolving to the value returned by the executed delegate.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> source,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        var result = await source.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }
}
