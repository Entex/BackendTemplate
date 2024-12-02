using BackendTemplate.Domain.Entities.Base;

namespace BackendTemplate.Domain.Entities.User;

public class UserRole : Entity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ICollection<UserPermission> Permissions { get; private set; } =
        new List<UserPermission>();

    public UserRole(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void AddPermission(UserPermission permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }

    public void RemovePermission(UserPermission permission)
    {
        if (Permissions.Contains(permission))
        {
            Permissions.Remove(permission);
        }
    }
}
