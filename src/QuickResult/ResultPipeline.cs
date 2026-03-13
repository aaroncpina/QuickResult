using System.Threading.Tasks;
using System;

namespace QuickResult;

/// <summary>
/// Deferred source for composing Result pipelines.
/// </summary>
/// <typeparam name="T">Produced value type.</typeparam>
public readonly struct ResultPipeline<T>
{
    private readonly Func<Task<T>> _factory;

    internal ResultPipeline(Func<Task<T>> factory) => _factory = factory;

    internal Task<T> ExecuteAsync() => _factory();
}
