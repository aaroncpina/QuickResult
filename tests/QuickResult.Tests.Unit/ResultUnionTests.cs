namespace QuickResult.Tests.Unit;

public class ResultUnionTests
{
    private sealed record NotFoundError(string Message) : IError;

    [Fact]
    public void Switch_SelectsValueArm_WhenResultIsSuccess()
    {
        var result = Result.Success(42);

        var text = result switch
        {
            int value => $"value {value}",
            IError error => $"error {error.Message}"
        };

        Assert.Equal("value 42", text);
    }

    [Fact]
    public void Switch_SelectsErrorArm_WhenResultIsFailure()
    {
        var result = Result.Failure<int>("boom");

        var text = result switch
        {
            int value => $"value {value}",
            IError error => $"error {error.Message}"
        };

        Assert.Equal("error boom", text);
    }

    [Fact]
    public void Switch_SelectsSpecificErrorArm_WhenFailureHoldsCustomErrorType()
    {
        var result = Result.Failure<string>(new NotFoundError("no such user"));

        var text = result switch
        {
            string value => value,
            NotFoundError notFound => $"404 {notFound.Message}",
            IError error => $"500 {error.Message}"
        };

        Assert.Equal("404 no such user", text);
    }

    [Fact]
    public void Switch_SelectsNullArm_WhenSuccessValueIsNull()
    {
        var result = Result.Success<string?>(null);

        var text = result switch
        {
            string value => value,
            IError error => error.Message,
            null => "null value"
        };

        Assert.Equal("null value", text);
    }

    [Fact]
    public void Switch_SelectsNullArm_WhenSuccessValueIsNullNullableValueType()
    {
        var result = Result.Success<int?>(null);

        var text = result switch
        {
            int value => $"value {value}",
            IError error => error.Message,
            null => "null value"
        };

        Assert.Equal("null value", text);
    }

    [Fact]
    public void Switch_SelectsErrorArm_WhenSuccessTypeIsAlsoAnError()
    {
        var result = Result.Success<IError>(new Error("a value, not a failure"));

        var text = result switch
        {
            IError error => error.Message
        };

        Assert.True(result.IsSuccess);
        Assert.Equal("a value, not a failure", text);
    }

    [Fact]
    public void Switch_SelectsValueArm_WhenSuccessValueIsDefault()
    {
        var result = Result.Success(0);

        var text = result switch
        {
            int value => $"value {value}",
            IError error => error.Message
        };

        Assert.Equal("value 0", text);
    }

    [Fact]
    public void Switch_SelectsValueArm_WhenNullableValueTypeHasValue()
    {
        var result = Result.Success<int?>(5);

        var text = result switch
        {
            int value => $"value {value}",
            IError error => error.Message,
            null => "null value"
        };

        Assert.Equal("value 5", text);
    }

    [Fact]
    public void Switch_SelectsErrorArm_WhenFailureSuccessTypeIsAlsoAnError()
    {
        var result = Result.Failure<IError>("boom");

        var text = result switch
        {
            IError error => error.Message
        };

        Assert.True(result.IsFailure);
        Assert.Equal("boom", text);
    }

    [Fact]
    public void NullPattern_Matches_WhenSuccessValueIsNull()
    {
        var result = Result.Success<string?>(null);

        Assert.True(result.IsSuccess);
        Assert.True(result is null);
    }

    [Fact]
    public void Switch_SelectsErrorArm_WhenObjectSuccessValueIsAnError()
    {
        var result = Result.Success<object>(new Error("a value, not a failure"));

        var text = result switch
        {
            IError error => $"error {error.Message}",
            _ => "other"
        };

        Assert.Equal("error a value, not a failure", text);
    }

    [Fact]
    public void Switch_SelectsObjectArm_WhenObjectResultIsFailure()
    {
        var result = Result.Failure<object>("boom");

        var text = result switch
        {
            string value => value,
            object other => $"object {other}"
        };

        Assert.Equal("object boom", text);
    }

    [Fact]
    public void PatternMatching_DoesNotAllocate_WhenErrorArmIsTestedBeforeValueArm()
    {
        var result = Result.Success(7);
        long Sum()
        {
            long sum = 0;
            for (var i = 0; i < 10_000; i++)
            {
                sum += result switch
                {
                    IError => -1,
                    int value => value
                };
            }

            return sum;
        }
        Sum();

        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = Sum();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(70_000, total);
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void IsPattern_ExposesValue_WhenResultIsSuccess()
    {
        var result = Result.Success("hello");

        Assert.True(result is string value && value == "hello");
        Assert.False(result is IError);
    }

    [Fact]
    public void IsPattern_ExposesError_WhenResultIsFailure()
    {
        var result = Result.Failure<string>("boom");

        Assert.True(result is IError error && error.Message == "boom");
        Assert.False(result is string);
    }

    [Fact]
    public void PropertyPattern_StillReadsResultMembers_WhenResultIsUnion()
    {
        var success = Result.Success(5);
        var failure = Result.Failure<int>("boom");

        Assert.True(success is { IsSuccess: true, Value: 5 });
        Assert.True(failure is { IsFailure: true, Error.Message: "boom" });
    }

    [Fact]
    public void NullPattern_MatchesNullReference_WhenResultVariableIsNull()
    {
        Result<int>? missing = null;
        var present = Result.Success(1);

        Result<string>? missingReferenceResult = null;

        Assert.True(missing is null);
        Assert.True(missingReferenceResult is null);
        Assert.True(present is not null);
    }

    [Fact]
    public void ImplicitConversion_CreatesSuccess_WhenAssignedValue()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_CreatesFailure_WhenAssignedError()
    {
        Result<int> result = new NotFoundError("no such user");

        Assert.True(result.IsFailure);
        Assert.Equal(new NotFoundError("no such user"), result.Error);
    }

    [Fact]
    public void ImplicitConversion_CreatesFailure_WhenAssignedInterfaceTypedError()
    {
        IError error = new Error("boom");

        Result<int> result = error;

        Assert.True(result.IsFailure);
        Assert.Same(error, result.Error);
    }

    [Fact]
    public void ImplicitConversion_Throws_WhenErrorMessageIsBlank()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Result<int> result = new NotFoundError(" ");
            return result;
        });
    }

    [Fact]
    public void ImplicitConversion_SelectsBranch_WhenMethodReturnsValueOrError()
    {
        static Result<int> ParsePositive(string text)
        {
            if (int.TryParse(text, out var number) && number > 0)
            {
                return number;
            }

            return new Error("Not a positive integer");
        }

        Assert.Equal(7, ParsePositive("7").Value);
        Assert.Equal("Not a positive integer", ParsePositive("-1").Error.Message);
    }

    [Fact]
    public void PatternMatching_DoesNotAllocate_WhenSuccessValueIsValueType()
    {
        var result = Result.Success(7);
        long Sum()
        {
            long sum = 0;
            for (var i = 0; i < 10_000; i++)
            {
                sum += result switch
                {
                    int value => value,
                    IError => -1
                };
            }

            return sum;
        }
        Sum();

        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = Sum();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(70_000, total);
        Assert.Equal(0, allocated);
    }
}
