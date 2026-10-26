namespace BackendTemplate.Domain.Entities.Base;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? LastModifiedAt { get; protected set; }
}
