using BackendTemplate.Application.Features.Users.Commands;

using FluentValidation;

namespace BackendTemplate.Application.Features.Users.Validators;

public class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
