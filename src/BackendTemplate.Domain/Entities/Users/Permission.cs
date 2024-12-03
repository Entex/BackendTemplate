using BackendTemplate.Domain.Entities.Base;

namespace BackendTemplate.Domain.Entities.Users;

public class Permission : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public IList<Role> Roles { get; private set; } = [];
    public IList<User> Users { get; private set; } = [];

    public Permission(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
