using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public IList<Error> Errors { get; }

    protected Result(bool isSuccess, Error error, IList<Error> errors)
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
            if (error is null && (errors is null || errors.Count == 0))
            {
                throw new ArgumentException("Failure result must have at least one error");
            }
        }
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() => new Result(true, null, null);
    public static Result Failure(Error error) => new Result(false, error, null);
    public static Result Failure(IList<Error> errors) => new Result(false, null, errors);
}

public sealed class Result<T> : Result
{
    public T Value { get; }

    private Result(T value, bool isSuccess, Error error, IList<Error> errors)
        : base(isSuccess, error, errors)
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

public sealed record Error
{
    public string? Code { get; set; } = string.Empty;
    public string? UserDescription { get; set; } = string.Empty;
    public string DevDescription { get; set; }
    public object? StructuredLogContext { get; set; } = string.Empty;
    public static readonly Error None = new Error(string.Empty, string.Empty, string.Empty, null);

    public Error(string? code = null, string? userDesc = null, string devDesc = "", object? context = null)
    {
        Code = code;
        UserDescription = userDesc;
        DevDescription = devDesc;
        StructuredLogContext = context;
    }
}