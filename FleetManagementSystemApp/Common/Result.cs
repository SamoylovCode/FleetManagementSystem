namespace FleetManagementSystemApp.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }
    public IList<Error> Errors { get; }

    public Result(T value, bool isSuccess)
    {
        Value = value;
        IsSuccess = isSuccess;
    }

    public Result(T value, bool isSuccess, Error error) : this(value, isSuccess)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        Error = error;
    }

    public Result(T value, bool  isSuccess, IList<Error> errors) : this(value, isSuccess) 
    {
        if (isSuccess && errors.Count > 0  || !isSuccess && errors.Count > 0)
        {
            throw new ArgumentException("Invalid error", nameof(errors));
        }

        Errors = errors;
    }

    public static Result<T> Success(T value) => new Result<T>(value, true);

    public static Result<T> Failure(Error error) => new Result<T>(default, false, error);
    public static Result<T> Failure(IList<Error> errors) => new Result<T>(default, false, errors);
}

public sealed record Error
{
    public string Code { get; set; }
    public string Description { get; set; }
    public static readonly Error None = new Error(string.Empty, string.Empty);

    public Error(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public Error(Enum code, string description) : this(code.ToString(), description) { }
}