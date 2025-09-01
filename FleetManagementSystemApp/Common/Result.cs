using Newtonsoft.Json;

namespace FleetManagementSystemApp.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public IList<Error> Errors { get; }

    //[JsonConstructor]
    protected Result(
        bool isSuccess,
        Error? error,
        IList<Error>? errors)
    {
        if (isSuccess)
        {
            if (error is not null || (errors is not null && errors.Count > 0))
            {
                throw new ArgumentException("Success result cannot have errors");
            }
        }
        else
        {
            if ((errors?.Count ?? 0) == 0 && error is null)
            {
                throw new ArgumentException("Failure result must have at least one error");
            }
        }

        IsSuccess = isSuccess;

        if (isSuccess)
        {
            Error = null;
            Errors = Array.Empty<Error>();
        }
        else
        {
            Errors = (errors is { Count: > 0 }
                ? errors
                : new List<Error> { error! });
            Error = null;
        }
    }

    public static Result Success() => new Result(true, null, null);

    public static Result Failure(Error error) => new Result(false, error, null);

    public static Result Failure(IList<Error> errors) => new Result(false, null, errors);
}

public sealed class Result<T> : Result
{
    public T Value { get; }

    private Result(
        T value,
        bool isSuccess,
        Error error,
        IList<Error> errors) : base(isSuccess, error, errors)
    {
        if (isSuccess && value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success result must have a value");
        }

        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(value, true, null, null);

    public static Result<T> Failure(Error error) => new Result<T>(default, false, error, null);

    public static Result<T> Failure(IList<Error> errors) => new Result<T>(default, false, null, errors);
}