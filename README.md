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
| `IsSuccess` / `IsFailure` | Check result state |
| `Value` | Get success value (throws on failure) |
| `Error` | Get error message (throws on success) |
| `Match(...)` | Convert both branches to one value |
| `Map(...)` | Transform success value |
| `Bind(...)` | Chain result-returning functions |
| `left \| right` | Fallback to `right` if `left` failed |

---

## Examples

### 1) Create success/failure

```csharp
var ok = Result.Success(42);
var fail = Result.Failure("Something went wrong");
```

### 2) Match both paths

```csharp
string text = ok.Match(
    onSuccess: v => $"Value: {v}",
    onFailure: e => $"Error: {e}");
```

### 3) Map

```csharp
var length = Result.Success("hello").Map(s => s.Length); // Success(5)
```

### 4) Bind

```csharp
Result ParsePositiveInt(string input) =>
    int.TryParse(input, out var n) && n > 0
        ? Result.Success(n)
        : Result.Failure("Input must be a positive integer");

var parsed = Result.Success("25").Bind(ParsePositiveInt);
```

### 5) Fallback with `|`

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
- `Fail(error)` throws `ArgumentException` if `error` is null/empty/whitespace.
- Failure short-circuits through `Map`, `Bind`, and LINQ query chains.

---

## Target Frameworks

- .NET 8+
- C# 12+

---

## License

MIT
