namespace BackendTemplate.Application.Exceptions;

public class HttpException : Exception
{
    public int StatusCode { get; }

    public HttpException(int statusCode)
    {
        StatusCode = statusCode;
    }

    public HttpException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpException(int statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
