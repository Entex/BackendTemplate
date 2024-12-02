using BackendTemplate.Domain.Entities.Base;

namespace BackendTemplate.Domain.Entities.User;

public class UserPermission : Entity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public UserPermission(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
