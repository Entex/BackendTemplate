namespace BackendTemplate.Application.Exceptions;

public sealed class ConflictException(string message) : HttpException(409, message);
