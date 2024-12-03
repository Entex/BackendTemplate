using BackendTemplate.Application.Interface.Services;
using BackendTemplate.Domain.Events.Users;
using MediatR;

namespace BackendTemplate.Application.Features.Users.EventHandlers;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var to = notification.User.Email;
        var subject = "BackendTemplate - Account Created, please verify your email";
        var body =
            $"Hello {notification.User.FirstName}, your account has been created successfully."
            + "Please verify your email by clicking the link below."
            + "https://backendtemplate.com/verify-email?token=1234567890"
            + "Thank you for using BackendTemplate.";

        // Send email
        await _emailService.SendEmailAsync(to, subject, body);
    }
}
