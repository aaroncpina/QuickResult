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

## Core API

| Member | Purpose |
|---|---|
| `Result<T>.Success(value)` | Create a successful result |
| `Result<T>.Failure(error)` | Create a failed result |
| `Result.FromNullable(value, error)` | Convert nullable reference/value types into `Result<T>` |
| `IsSuccess` / `IsFailure` | Check result state |
| `Value` | Get success value (throws on failure) |
| `Error` | Get error message (throws on success) |
| `Match(...)` | Convert both branches to one value |
| `MatchAsync(...)` | Async version of match (supports async branches) |
| `ValueOr(...)` | Get value or fallback |
| `Map(...)` | Transform success value |
| `MapFailure(...)` | Transform failure message |
| `Bind(...)` | Chain result-returning functions |
| `Result.Try(...)` | Wrap sync code in try/catch and return `Result<T>` |
| `Result.TryAsync(...)` | Wrap async code in try/catch and return `Task<Result<T>>` |
| `left \| right` | Fallback to `right` if `left` failed |

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

var loaded = await Result.TryAsync(async () => { await Task.Delay(10); return "done"; }); // Success("done")
```

### 4.1) Match both paths

```csharp
string text = ok.Match(
    onSuccess: v => $"Value: {v}",
    onFailure: e => $"Error: {e}");
```

### 4.2) Match async branches

```csharp
var text = await ok.MatchAsync(
    onSuccess: v => Task.FromResult("Value: {v}"),
    onFailure: e => Task.FromResult("Error: {e}"));
```

### 5.1) Map

```csharp
var length = Result.Success("hello").Map(s => s.Length); // Success(5)
```

### 5.2) Map failure

```csharp
var mappedError = Result.Failure("boom").MapFailure(e => $"wrapped: {e}"); // Failure("wrapped: boom")
```

### 6) ValueOr

```csharp
var value1 = Result.Success(10).ValueOr(0); // 10
var value2 = Result.Failure("bad").ValueOr(0); // 0
var value3 = Result.Failure("bad").ValueOr(e => e.Length); // 3
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
- `Failure(error)` throws `ArgumentException` if `error` is null/empty/whitespace.
- `Result.FromNullable(value, error)` returns `Failure(error)` when the nullable input is null.
- `Result.Try(...)` and `Result.TryAsync(...)` convert thrown exceptions into `Failure(...)` using the exception message.
- If an exception message is null/whitespace, `Try/TryAsync` use the exception type name as the failure message.
- Failure short-circuits through `Map`, `Bind`, and LINQ query chains.

---

## Target Frameworks

- .NET 8+
- C# 12+

---

## License

MIT
