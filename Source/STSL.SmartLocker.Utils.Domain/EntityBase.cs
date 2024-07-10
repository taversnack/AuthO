namespace STSL.SmartLocker.Utils.Domain;

public abstract class EntityBase
{
    public Guid TenantId { get; set; }
}

public abstract class EntityBaseWithTenant : EntityBase
{
    public Tenant? Tenant { get; set; }
}

public interface IUsesGuidId
{
    Guid Id { get; set; }
}

public interface IUsesSoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public interface IUsesAuditing
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}