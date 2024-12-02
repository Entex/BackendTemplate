namespace BackendTemplate.Domain.ValueObjects;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Error { get; }

    protected Result(bool isSuccess, T? value, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("Successful result cannot have an error message.");

        if (!isSuccess && value is not null)
            throw new InvalidOperationException("Failed result cannot have a value.");

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public static implicit operator bool(Result<T> result)
    {
        return result.IsSuccess;
    }
}
