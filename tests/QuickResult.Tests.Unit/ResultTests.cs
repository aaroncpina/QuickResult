namespace QuickResult.Tests.Unit;

public class ResultTests
{
    [Fact]
    public void NonGenericFactory_Success_InfersType()
    {
        var result = Result.Success(123);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(123, result.Value);
    }

    [Fact]
    public void NonGenericFactory_SuccessWithoutValue_ReturnsUnitSuccess()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(QuickResult.Unit.Value, result.Value);
    }

    [Fact]
    public void Try_Action_WhenSucceeds_ReturnsUnitSuccess()
    {
        void Called() { }

        var result = Result.Try(Called);

        Assert.True(result.IsSuccess);
        Assert.Equal(QuickResult.Unit.Value, result.Value);
    }

    [Fact]
    public void Try_Action_WhenThrows_ReturnsUnitFailure()
    {
        var result = Result.Try(() => throw new InvalidOperationException("boom"));

        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public async Task TryAsync_Task_WhenSucceeds_ReturnsUnitSuccess()
    {
        var called = false;

        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            called = true;
        });

        Assert.True(called);
        Assert.True(result.IsSuccess);
        Assert.Equal(QuickResult.Unit.Value, result.Value);
    }

    [Fact]
    public async Task TryAsync_Task_WhenThrows_ReturnsUnitFailure()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("boom");
        });

        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void FromNullable_Reference_WhenNotNull_ReturnsSuccess()
    {
        string? value = "docker";

        var result = Result.FromNullable(value, "missing");

        Assert.True(result.IsSuccess);
        Assert.Equal("docker", result.Value);
    }

    [Fact]
    public void FromNullable_Reference_WhenNull_ReturnsFailure()
    {
        string? value = null;

        var result = Result.FromNullable(value, "missing");

        Assert.True(result.IsFailure);
        Assert.Equal("missing", result.Error);
    }

    [Fact]
    public void FromNullable_ValueType_WhenHasValue_ReturnsSuccess()
    {
        int? value = 42;

        var result = Result.FromNullable(value, "missing");

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FromNullable_ValueType_WhenNull_ReturnsFailure()
    {
        int? value = null;

        var result = Result.FromNullable(value, "missing");

        Assert.True(result.IsFailure);
        Assert.Equal("missing", result.Error);
    }

    [Fact]
    public void Try_WhenFunctionSucceeds_ReturnsSuccess()
    {
        var result = Result.Try(() => 42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_WhenFunctionThrows_ReturnsFailureWithExceptionMessage()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("boom"));

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public async Task TryAsync_GenericTask_WhenSucceeds_ReturnsSuccess()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TryAsync_GenericTask_WhenThrows_ReturnsFailure()
    {
        var result = await Result.TryAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("boom");
        });

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void NonGenericFactory_Failure_CreatesFailureResult()
    {
        var result = Result.Failure<int>("boom");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Failure("no value");

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Error_OnSuccess_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Success(10);

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void Match_OnSuccess_UsesSuccessFunc()
    {
        var result = Result<int>.Success(5);

        var text = result.Match(
            onSuccess: v => $"ok:{v}",
            onFailure: e => $"err:{e}");

        Assert.Equal("ok:5", text);
    }

    [Fact]
    public void Match_OnFailure_UsesFailureFunc()
    {
        var result = Result<int>.Failure("bad");

        var text = result.Match(
            onSuccess: v => $"ok:{v}",
            onFailure: e => $"err:{e}");

        Assert.Equal("err:bad", text);
    }

    [Fact]
    public async Task MatchAsync_BothAsync_OnSuccess_UsesSuccessFunc()
    {
        var result = Result<int>.Success(5);

        var text = await result.MatchAsync(
            onSuccess: v => Task.FromResult($"ok:{v}"),
            onFailure: e => Task.FromResult($"err:{e}"));

        Assert.Equal("ok:5", text);
    }

    [Fact]
    public async Task MatchAsync_BothAsync_OnFailure_UsesFailureFunc()
    {
        var result = Result<int>.Failure("bad");

        var text = await result.MatchAsync(
            onSuccess: v => Task.FromResult($"ok:{v}"),
            onFailure: e => Task.FromResult($"err:{e}"));

        Assert.Equal("err:bad", text);
    }

    [Fact]
    public async Task MatchAsync_AsyncSuccess_SyncFailure_Works()
    {
        var result = Result<int>.Failure("bad");

        var text = await result.MatchAsync(
            onSuccess: v => Task.FromResult($"ok:{v}"),
            onFailure: e => $"err:{e}");

        Assert.Equal("err:bad", text);
    }

    [Fact]
    public async Task MatchAsync_SyncSuccess_AsyncFailure_Works()
    {
        var result = Result<int>.Success(5);

        var text = await result.MatchAsync(
            onSuccess: v => $"ok:{v}",
            onFailure: e => Task.FromResult($"err:{e}"));

        Assert.Equal("ok:5", text);
    }

    [Fact]
    public void MapFailure_OnFailure_MapsError()
    {
        var result = Result<int>.Failure("boom");

        var mapped = result.MapFailure(e => $"wrapped:{e}");

        Assert.True(mapped.IsFailure);
        Assert.Equal("wrapped:boom", mapped.Error);
    }

    [Fact]
    public void MapFailure_OnSuccess_PreservesSuccess()
    {
        var result = Result<int>.Success(7);

        var mapped = result.MapFailure(e => $"wrapped:{e}");

        Assert.True(mapped.IsSuccess);
        Assert.Equal(7, mapped.Value);
    }

    [Fact]
    public void ValueOr_OnSuccess_ReturnsValue()
    {
        var result = Result<int>.Success(99);

        var value = result.ValueOr(10);

        Assert.Equal(99, value);
    }

    [Fact]
    public void ValueOr_OnFailure_ReturnsFallback()
    {
        var result = Result<int>.Failure("no value");

        var value = result.ValueOr(10);

        Assert.Equal(10, value);
    }

    [Fact]
    public void ValueOrFactory_OnFailure_UsesError()
    {
        var result = Result<int>.Failure("bad");

        var value = result.ValueOr(e => e.Length);

        Assert.Equal(3, value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesFailure()
    {
        var result = Result<int>.Failure("map fail");

        var mapped = result.Map(v => v * 10);

        Assert.True(mapped.IsFailure);
        Assert.Equal("map fail", mapped.Error);
    }

    [Fact]
    public void Bind_OnSuccess_BindsToNextResult()
    {
        var result = Result<int>.Success(8);

        var bound = result.Bind(v => Result<string>.Success($"v:{v}"));

        Assert.True(bound.IsSuccess);
        Assert.Equal("v:8", bound.Value);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesFailure()
    {
        var result = Result<int>.Failure("bind fail");

        var bound = result.Bind(v => Result<string>.Success($"v:{v}"));

        Assert.True(bound.IsFailure);
        Assert.Equal("bind fail", bound.Error);
    }

    [Fact]
    public void PipeOperator_ReturnsLeft_WhenLeftIsSuccess()
    {
        var left = Result<int>.Success(1);
        var right = Result<int>.Success(2);

        var chosen = left | right;

        Assert.True(chosen.IsSuccess);
        Assert.Equal(1, chosen.Value);
    }

    [Fact]
    public void PipeOperator_ReturnsRight_WhenLeftIsFailure_AndRightIsSuccess()
    {
        var left = Result<int>.Failure("left fail");
        var right = Result<int>.Success(2);

        var chosen = left | right;

        Assert.True(chosen.IsSuccess);
        Assert.Equal(2, chosen.Value);
    }

    [Fact]
    public void PipeOperator_ReturnsRight_WhenLeftIsFailure_AndRightIsFailure()
    {
        var left = Result<int>.Failure("left fail");
        var right = Result<int>.Failure("right fail");

        var chosen = left | right;

        Assert.True(chosen.IsFailure);
        Assert.Equal("right fail", chosen.Error);
    }

    [Fact]
    public void Linq_SyncQuery_Succeeds()
    {
        var query =
            from a in Result<int>.Success(10)
            from b in Result<int>.Success(5)
            select a + b;

        Assert.True(query.IsSuccess);
        Assert.Equal(15, query.Value);
    }

    [Fact]
    public void Linq_SyncQuery_PropagatesFailure()
    {
        var query =
            from a in Result<int>.Success(10)
            from b in Result<int>.Failure("sync fail")
            select a + b;

        Assert.True(query.IsFailure);
        Assert.Equal("sync fail", query.Error);
    }

    [Fact]
    public async Task Linq_AsyncQuery_Succeeds()
    {
        var query =
            from a in GetSuccessAsync(7)
            from b in GetSuccessAsync(3)
            select a * b;

        var result = await query;

        Assert.True(result.IsSuccess);
        Assert.Equal(21, result.Value);
    }

    [Fact]
    public async Task Linq_AsyncQuery_PropagatesFailure()
    {
        var query =
            from a in GetSuccessAsync(7)
            from b in GetFailureAsync<int>("async fail")
            select a * b;

        var result = await query;

        Assert.True(result.IsFailure);
        Assert.Equal("async fail", result.Error);
    }

    [Fact]
    public async Task Linq_Mixed_SyncThenAsyncThenSync_Succeeds()
    {
        var query =
            from a in Result<int>.Success(4)       // sync source
            from b in GetSuccessAsync(6)           // async bind
            from c in Result<int>.Success(2)       // sync bind after async
            select a + b + c;

        var result = await query;

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value);
    }

    [Fact]
    public async Task Linq_Mixed_AsyncThenSync_PropagatesFailure()
    {
        var query =
            from a in GetSuccessAsync(4)                   // async source
            from b in Result<int>.Failure("mixed fail")    // sync bind
            select a + b;

        var result = await query;

        Assert.True(result.IsFailure);
        Assert.Equal("mixed fail", result.Error);
    }

    [Fact]
    public async Task Pipeline_From_Try_WhenSucceeds_ReturnsSuccess()
    {
        var result = await Result.From(() => 21).Try();

        Assert.True(result.IsSuccess);
        Assert.Equal(21, result.Value);
    }

    [Fact]
    public async Task Pipeline_From_Try_WhenThrows_ReturnsFailure()
    {
        var result = await Result.From<int>(() => throw new InvalidOperationException("boom")).Try();

        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public async Task Pipeline_FromAsync_Try_WhenSucceeds_ReturnsSuccess()
    {
        var result = await Result.FromAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        }).Try();

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task Pipeline_FromAsync_Try_WhenThrows_ReturnsFailure()
    {
        var result = await Result.FromAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("boom");
        }).Try();

        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public async Task Pipeline_WhenNull_Reference_WhenNull_ReturnsFailure()
    {
        var result = await Result.FromAsync(() => Task.FromResult<string?>(null))
                                 .Try()
                                 .WhenNull("missing");

        Assert.True(result.IsFailure);
        Assert.Equal("missing", result.Error);
    }

    [Fact]
    public async Task Pipeline_WhenNull_Reference_WhenNotNull_ReturnsSuccess()
    {
        var result = await Result.FromAsync(() => Task.FromResult<string?>("ok"))
                                 .Try()
                                 .WhenNull("missing");

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Pipeline_WhenNull_ValueType_WhenNull_ReturnsFailure()
    {
        var result = await Result.FromAsync(() => Task.FromResult<int?>(null))
                                 .Try()
                                 .WhenNull("missing");

        Assert.True(result.IsFailure);
        Assert.Equal("missing", result.Error);
    }

    [Fact]
    public async Task Pipeline_WhenNull_ValueType_WhenHasValue_ReturnsSuccess()
    {
        var result = await Result.FromAsync(() => Task.FromResult<int?>(7))
                                 .Try()
                                 .WhenNull("missing");

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public async Task Pipeline_Linq_ComposesTryAndWhenNull()
    {
        var query =
            from a in Result.FromAsync(() => Task.FromResult<int?>(4)).Try().WhenNull("a missing")
            from b in Result.FromAsync(() => Task.FromResult<int?>(6)).Try().WhenNull("b missing")
            select a + b;

        var result = await query;

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task Pipeline_Linq_PropagatesWhenNullFailure()
    {
        var query =
            from a in Result.FromAsync(() => Task.FromResult<int?>(4)).Try().WhenNull("a missing")
            from b in Result.FromAsync(() => Task.FromResult<int?>(null)).Try().WhenNull("b missing")
            select a + b;

        var result = await query;

        Assert.True(result.IsFailure);
        Assert.Equal("b missing", result.Error);
    }

    private static Task<Result<int>> GetSuccessAsync(int value) =>
        Task.FromResult(Result<int>.Success(value));

    private static Task<Result<T>> GetFailureAsync<T>(string error) =>
        Task.FromResult(Result<T>.Failure(error));
}
