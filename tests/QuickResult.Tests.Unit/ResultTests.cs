namespace QuickResult.Tests.Unit;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var result = Result<int>.Success(123);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(123, result.Value);
    }

    [Fact]
    public void Fail_CreatesFailureResult()
    {
        var result = Result<int>.Fail("boom");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void Fail_WithWhitespaceError_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result<int>.Fail(" "));
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Fail("no value");

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
        var result = Result<int>.Fail("bad");

        var text = result.Match(
            onSuccess: v => $"ok:{v}",
            onFailure: e => $"err:{e}");

        Assert.Equal("err:bad", text);
    }

    [Fact]
    public void Map_OnSuccess_MapsValue()
    {
        var result = Result<int>.Success(3);

        var mapped = result.Map(v => v * 10);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(30, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesFailure()
    {
        var result = Result<int>.Fail("map fail");

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
        var result = Result<int>.Fail("bind fail");

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
        var left = Result<int>.Fail("left fail");
        var right = Result<int>.Success(2);

        var chosen = left | right;

        Assert.True(chosen.IsSuccess);
        Assert.Equal(2, chosen.Value);
    }

    [Fact]
    public void PipeOperator_ReturnsRight_WhenLeftIsFailure_AndRightIsFailure()
    {
        var left = Result<int>.Fail("left fail");
        var right = Result<int>.Fail("right fail");

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
            from b in Result<int>.Fail("sync fail")
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
            from b in Result<int>.Fail("mixed fail")       // sync bind
            select a + b;

        var result = await query;

        Assert.True(result.IsFailure);
        Assert.Equal("mixed fail", result.Error);
    }

    private static Task<Result<int>> GetSuccessAsync(int value) =>
        Task.FromResult(Result<int>.Success(value));

    private static Task<Result<T>> GetFailureAsync<T>(string error) =>
        Task.FromResult(Result<T>.Fail(error));
}
