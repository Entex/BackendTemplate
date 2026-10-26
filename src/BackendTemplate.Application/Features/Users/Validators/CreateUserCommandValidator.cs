using BackendTemplate.Application.Features.Users.Commands;
using BackendTemplate.Domain.ValueObjects;
using FluentValidation;

namespace BackendTemplate.Application.Features.Users.Validators;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().Must(value => Email.Create(value).IsSuccess)
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}
