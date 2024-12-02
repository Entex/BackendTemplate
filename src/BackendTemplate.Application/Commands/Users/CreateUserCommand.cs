namespace BackendTemplate.Application.Commands.Users;

public record CreateUserCommand(string FirstName, string LastName, string Email, string Password)
    : IRequest<Guid>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEventBus eventBus
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            _passwordHasher.HashPassword(request.Password)
        );
        await _userRepository.AddAsync(user, cancellationToken);
        await _eventBus.PublishAsync(new UserRegisteredEvent(user.Id), cancellationToken);
        return user.Id;
    }
}
