namespace BackendTemplate.Application.Exceptions;

public sealed class ForbiddenException() : HttpException(403, "You do not have permission to perform this action.");
