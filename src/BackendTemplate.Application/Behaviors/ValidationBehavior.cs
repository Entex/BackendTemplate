using BackendTemplate.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace BackendTemplate.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        // Validators can share scoped dependencies such as a DbContext.
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
            throw new Exceptions.ValidationException(failures);

        return await next(request, cancellationToken);
    }
}
