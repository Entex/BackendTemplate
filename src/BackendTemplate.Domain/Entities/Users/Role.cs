using BackendTemplate.Domain.Entities.Base;

namespace BackendTemplate.Domain.Entities.Users;

public class Role : Entity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public IList<User> Users { get; private set; } = [];
    public IList<Permission> Permissions { get; private set; } = [];

    public Role(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddPermission(Permission permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }

    public void RemovePermission(Permission permission)
    {
        Permissions.Remove(permission);
    }
}
