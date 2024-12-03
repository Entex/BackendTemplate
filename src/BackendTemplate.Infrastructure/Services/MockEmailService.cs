using BackendTemplate.Application.Interface.Services;

namespace BackendTemplate.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        Console.WriteLine($"Email sent to {to} with subject '{subject}' and body: {body}");
        return Task.CompletedTask;
    }
}
