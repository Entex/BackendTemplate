using BackendTemplate.Application.Features.Users.Commands;
using BackendTemplate.Domain.ValueObjects;
using FluentValidation;

namespace BackendTemplate.Application.Features.Users.Validators;

public sealed class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().Must(value => Email.Create(value).IsSuccess)
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
