namespace BackendTemplate.Application.Exceptions;

public class NotFoundException : HttpException
{
    public NotFoundException(string name, object key)
        : base(404, $"Entity \"{name}\" ({key}) was not found.") { }
}
