# QuickResult

Lightweight `Result<T>` for C# with full LINQ query syntax support for **sync**, **async**, and **mixed sync/async** flows.

[![NuGet](https://img.shields.io/nuget/v/QuickResult.svg)](https://www.nuget.org/packages/QuickResult)
[![NuGet Downloads](https://img.shields.io/nuget/dt/QuickResult.svg)](https://www.nuget.org/packages/QuickResult)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## Why QuickResult?

`Result<T>` makes expected failures explicit and composable.

- ✅ Avoid exceptions for normal control flow
- ✅ Keep happy-path and error-path close together
- ✅ Compose operations with `Map`, `Bind`, and LINQ queries
- ✅ Works naturally with async workflows
- ✅ Extensible error types via `IError` — use pattern matching (`is`, `as`, `switch`) on failures

---

## Install

```bash
dotnet add package QuickResult
```

---

## Quick Start

```csharp
using QuickResult;

var result = Result.Success(10).Map(x => x * 2);
var message = result.Match(
    onSuccess: value => $"OK: {value}",
    onFailure: error => $"FAIL: {error}");
```

---

## Custom Error Types

Implement `IError` to create domain-specific errors:

```csharp
public class HttpResponseError : IError
{
    public string Message { get; }
    public int StatusCode { get; }

    public HttpResponseError(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message    = message;
    }
}

// Use it directly:
var result = Result.Failure(new HttpResponseError(503, "Service unavailable"));

// Transform failures within a pipeline
var mapped = result.MapFailure(e => e switch
{
    HttpResponseError { StatusCode: 503 } => new Error("Try again later"),
    _                                     => e
});

// Or pattern match when consuming the error
if (result.IsFailure && result.Error is HttpResponseError http)
    Console.WriteLine($"HTTP {http.StatusCode}: {http.Message}");
```

Plain strings still work — they're implicitly wrapped in the built-in `Error` class:

```csharp
var fail = Result<int>.Failure("Something went wrong"); // just works
```

---

## Core API

| Member | Purpose |
|---|---|
| `Result<T>.Success(value)` | Create a successful result |
| `Result.Success()` | Create a successful `Result<Unit>` |
| `Result<T>.Failure(error)` | Create a failed result (accepts `IError` or `string`) |
| `Result.FromNullable(value, error)` | Convert nullable reference/value types into `Result<T>` |
| `IsSuccess` / `IsFailure` | Check result state |
| `Value` | Get success value (throws on failure) |
| `Error` | Get `IError` (throws on success) |
| `Match(...)` | Convert both branches to one value |
| `MatchAsync(...)` | Async version of match (supports async branches) |
| `ValueOr(...)` | Get value or fallback |
| `Map(...)` | Transform success value |
| `MapFailure(...)` | Transform failure error |
| `Bind(...)` | Chain result-returning functions |
| `Result.Try(...)` | Wrap sync `Func<T>` or `Action` in try/catch and return result |
| `Result.TryAsync(...)` | Wrap async `Func<Task<T>>` or `Func<Task>` in try/catch and return result |
| `left \| right` | Fallback to `right` if `left` failed |
| `Result.From(...)` / `Result.FromAsync(...)` | Start a composable pipeline from sync/async value factories |
| `pipeline.Try()` | Execute pipeline and convert thrown exceptions into `Failure(...)` |
| `result.WhenNull(error)` | Convert successful nullable result into `Failure(error)` when null |

---

## Examples

### 1) Create success/failure

```csharp
var ok = Result.Success(42);
var fail = Result.Failure("Something went wrong");
```

### 2) From nullable

```csharp
string? name = GetNameOrNull();
var nameResult = Result.FromNullable(name, "Name was missing");

int? port = GetPortOrNull();
var portResult = Result.FromNullable(port, "Port was missing");
```

### 3) Try / TryAsync

```csharp
var parsed = Result.Try(() => int.Parse("123")); // Success(123)
var failed = Result.Try(() => int.Parse("abc")); // Failure("...")

var sideEffect = Result.Try(() => Console.WriteLine("done")); // Result

var loaded = await Result.TryAsync(async () => { await Task.Delay(10); return "done"; }); // Success("done")

var ping = await Result.TryAsync(async () => { await Task.Delay(10); }); // Result
```

### 4.1) Match both paths

```csharp
string text = ok.Match(
    onSuccess: v => $"Value: {v}",
    onFailure: e => $"Error: {e.Message}");
```

### 4.2) Match async branches (`Result<T>` source)

```csharp
var text = await ok.MatchAsync(
    onSuccess: v => Task.FromResult("Value: {v}"),
    onFailure: e => Task.FromResult("Error: {e.Message}"));
```

### 4.3) Match directly on `Task<Result<T>>` (fluent)

```csharp
var text = await GetSuccessAsync(11).MatchAsync(
    onSuccess: v => "success: {v}",
    onFailure: e => "failure: {e.Message}");
```

### 5.1) Map

```csharp
var length = Result.Success("hello").Map(s => s.Length); // Success(5)
```

### 5.2) Map failure

```csharp
var mappedError = Result.Failure("boom").MapFailure(e => new Error( $"wrapped: {e.Message}")); // Failure("wrapped: boom")
```

### 6) ValueOr

```csharp
var value1 = Result.Success(10).ValueOr(0); // 10
var value2 = Result.Failure("bad").ValueOr(0); // 0
var value3 = Result.Failure("bad").ValueOr(e => e.Message.Length); // 3
```

### 7) Bind

```csharp
Result ParsePositiveInt(string input) =>
    int.TryParse(input, out var n) && n > 0
        ? Result.Success(n)
        : Result.Failure("Input must be a positive integer");

var parsed = Result.Success("25").Bind(ParsePositiveInt);
```

### 8) Fallback with `|`

```csharp
var chosen = Result.Failure("primary failed") | Result.Success(10); // Success(10)
```

### 9.1) Pipeline start + Try + WhenNull

```csharp
var userResult = await Result.FromAsync(() => repository.GetUserByIdAsync(userId, ct))
                             .Try()
                             .WhenNull("User not found");

// Works with Task<T>
var p1 = Result.FromAsync(() => SomeCallAsync());

// Also works with ConfigureAwait(...)
var p2 = Result.FromAsync(() => SomeCallAsync().ConfigureAwait(false));
```

### 9.2) LINQ query with pipelines

```csharp
var query = from val1 in Result.FromAsync(() => service.GetValue1Async(ct))
                               .Try()
                               .WhenNull("Value1 was null")
            from val2 in Result.FromAsync(() => service.GetValue2Async(ct))
                               .Try()
                               .WhenNull("Value2 was null")
            select val1 + val2;

var result = await query;
```

---

## Boolean guards in LINQ queries

When your query includes a boolean step (for example, "does user own documents?"), you can short-circuit with domain-specific errors using:

- `FailIfTrue(error)`
- `FailIfFalse(error)`

These methods work on `Result<bool>` and `Task<Result<bool>>`, so they compose cleanly in LINQ query syntax.

```csharp
Task<Result> CheckAsync() =>
    Task.FromResult(Result.Success(true));

Task NextAsync() =>
    Task.FromResult(Result.Success(42));

var query = from ok in CheckAsync().FailIfFalse("Precondition failed")
            from value in NextAsync()
            select value * 2;

var result = await query; // Success(84)
```

### Why not use `where`?

C# query `where` only accepts a boolean predicate and cannot carry a domain error message.
For `Result` pipelines, explicit boolean guards (`FailIfTrue` / `FailIfFalse`) keep error handling clear and intentional.

---

## LINQ Support

### Sync query

```csharp
var query = from a in Result.Success(10)
            from b in Result.Success(5)
            select a + b; // Success(15)
```

### Async query

```csharp
static Task<Result> GetAsync(int n) =>
    Task.FromResult(Result.Success(n));

var query = from a in GetAsync(4)
            from b in GetAsync(5)
            select a * b;

var result = await query; // Success(20)
```

### Async query + fluent MatchAsync

```csharp
var text = await (from left in GetAsync(11)
                  from right in GetAsync(31)
                  select left + right)
                 .MatchAsync(
                      onSuccess: v => "success: {v}",
                      onFailure: e => "failure: {e.Message}");
```

### Mixed sync/async query

```csharp
var query = from a in Result.Success(4) // sync
            from b in GetAsync(6)       // async
            from c in Result.Success(2) // sync
            select a + b + c;

var result = await query; // Success(12)
```

---

## Behavior Notes

- `Value` throws `InvalidOperationException` when result is failure.
- `Error` throws `InvalidOperationException` when result is success.
- `Failure(error)` throws `ArgumentException` if error message is null/empty/whitespace.
- `Failure(string)` implicitly wraps the string in the default `Error` type.
- `Result.FromNullable(value, error)` returns `Failure(error)` when the nullable input is null.
- `Result.Try(...)` and `Result.TryAsync(...)` convert thrown exceptions into `Failure(...)` using the exception message.
- `Try/TryAsync` support both value-returning and no-value (`Unit`) operations.
- If an exception message is null/whitespace, `Try/TryAsync` use the exception type name as the failure message.
- Failure short-circuits through `Map`, `Bind`, and LINQ query chains.
- `Result.From/FromAsync` create deferred pipelines. Pair with `.Try()` to convert thrown exceptions into failures.
- `WhenNull(error)` converts a successful nullable result into `Failure(error)` when the value is null.

---

## Target Frameworks

- .NET 8+
- C# 12+

---

## License

MIT
