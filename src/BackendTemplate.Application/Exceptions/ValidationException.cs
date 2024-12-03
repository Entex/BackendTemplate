using FluentValidation.Results;

namespace BackendTemplate.Application.Exceptions;

public class ValidationException : HttpException
{
    public IDictionary<string, List<string>>? Errors { get; }

    public ValidationException(string message)
        : base(400, message) { }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToList());
    }
}
