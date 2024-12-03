namespace BackendTemplate.Application.Exceptions;

public class UnauthorizedException : HttpException
{
    public UnauthorizedException()
        : base(401, "Unauthorized access.") { }

    public UnauthorizedException(string message)
        : base(401, message) { }
}
