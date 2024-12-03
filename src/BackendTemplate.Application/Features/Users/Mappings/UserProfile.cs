using AutoMapper;

using BackendTemplate.Application.Features.Users.Commands;
using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Features.Users.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserCommand, User>();
    }
}
