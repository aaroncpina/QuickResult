// C# union support. It relies on static abstract interface members, which .NET Standard 2.0
// does not have, so the .NET Standard build of Result<T> is not a union.
#if NET
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace QuickResult;

/// <remarks>
/// On .NET 10 and later, <see cref="Result{T}"/> is a C# union of <typeparamref name="T"/> and
/// <see cref="IError"/>. With a compiler that supports unions (C# 15 or later) a result can be
/// matched with type patterns, with exhaustiveness checked by the compiler, and a value or an
/// error converts implicitly to a result.
/// <code>
/// string text = result switch
/// {
///     int value => $"Got {value}",
///     IError error => $"Failed: {error.Message}"
/// };
/// </code>
/// A type pattern cannot tell success from failure when <typeparamref name="T"/> is itself
/// <see cref="IError"/> or <see cref="object"/>; use <see cref="IsSuccess"/> or <c>Match</c> there.
/// </remarks>
[Union]
public sealed partial class Result<T> : Result<T>.IUnionMembers
{
    object? IUnionMembers.Value => IsSuccess ? _value : _error;

    // The null checks below are written as branches on purpose. As a branch, the runtime drops the
    // check for a value-type T; written as an expression ("_value is not null") it boxes the value
    // in code the JIT has not yet optimised.
    bool IUnionMembers.HasValue
    {
        get
        {
            if (_value is null)
            {
                return IsFailure;
            }

            return true;
        }
    }

    // The typed accessors must agree with IUnionMembers.Value for every T, including the cases
    // where T and IError overlap (T is IError, object, or an error type): a failure whose error
    // is a T answers the T accessor, and a success whose value is an IError answers the IError one.
    bool IUnionMembers.TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (IsFailure)
        {
            if (_error is T errorAsValue)
            {
                value = errorAsValue;
                return true;
            }

            value = default;
            return false;
        }

        if (_value is null)
        {
            value = default;
            return false;
        }

        value = _value;
        return true;
    }

    bool IUnionMembers.TryGetValue([MaybeNullWhen(false)] out IError error)
    {
        if (IsFailure)
        {
            error = _error!;
            return true;
        }

        // Skipped outright for value types that cannot be an IError, so they are never boxed here.
        error = SuccessValueCanBeError ? _value as IError : null;
        return error is not null;
    }

    private static readonly bool SuccessValueCanBeError =
        !typeof(T).IsValueType
        || typeof(IError).IsAssignableFrom(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));

    static Result<T> IUnionMembers.Create(T value) => Success(value);

    static Result<T> IUnionMembers.Create(IError error) => Failure(error);

    /// <summary>
    /// Describes <see cref="Result{T}"/> to the C# compiler as a union of
    /// <typeparamref name="T"/> and <see cref="IError"/>.
    /// </summary>
    /// <remarks>
    /// This interface is compiler infrastructure and is not intended to be used directly.
    /// Use pattern matching on the result, or <see cref="IsSuccess"/>, <see cref="Result{T}.Value"/>
    /// and <see cref="Error"/>.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IUnionMembers
    {
        /// <summary>
        /// Gets the success value or the failure error as an object. Boxes a value-type success value.
        /// </summary>
        object? Value { get; }

        /// <summary>
        /// Gets a value indicating whether the result holds a non-null success value or an error.
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Gets what the result holds as a <typeparamref name="T"/>, without boxing.
        /// </summary>
        /// <param name="value">The held value when the method returns <see langword="true"/>.</param>
        /// <returns>
        /// <see langword="true"/> when the result holds a non-null <typeparamref name="T"/>: a non-null
        /// success value, or a failure whose error is itself a <typeparamref name="T"/>.
        /// </returns>
        bool TryGetValue([MaybeNullWhen(false)] out T value);

        /// <summary>
        /// Gets what the result holds as an <see cref="IError"/>.
        /// </summary>
        /// <param name="error">The held error when the method returns <see langword="true"/>.</param>
        /// <returns>
        /// <see langword="true"/> when the result holds an <see cref="IError"/>: a failure, or a
        /// success whose value is itself an error.
        /// </returns>
        bool TryGetValue([MaybeNullWhen(false)] out IError error);

        /// <summary>
        /// Creates a successful result. Used by the compiler for implicit conversion from a value.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
        static abstract Result<T> Create(T value);

        /// <summary>
        /// Creates a failed result. Used by the compiler for implicit conversion from an error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
        static abstract Result<T> Create(IError error);
    }
}
#endif
