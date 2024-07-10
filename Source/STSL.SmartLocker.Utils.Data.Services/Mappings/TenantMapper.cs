using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class TenantMapper : IMapsToDTO<TenantDTO, Tenant>
{
    public static TenantDTO ToDTO(Tenant entity) => new
    (
        Id: entity.TenantId,
        Name: entity.Name,
        CardHolderAliasSingular: entity.CardHolderAliasSingular,
        CardHolderAliasPlural: entity.CardHolderAliasPlural,
        CardHolderUniqueIdentifierAlias: entity.CardHolderUniqueIdentifierAlias,
        HelpPortalUrl: entity.HelpPortalUrl,
        Logo: entity.Logo,
        LogoMimeType: entity.LogoMimeType,
        AllowLockUpdates: entity.AllowLockUpdates
    );
}

internal sealed class CreateTenantMapper : IMapsToEntity<CreateTenantDTO, Tenant>
{
    public static Tenant ToEntity(CreateTenantDTO dto)
    {
        var tenant = new Tenant
        {
            Name = dto.Name,
            AllowLockUpdates = dto.AllowLockUpdates,
        };

        tenant.CardHolderAliasSingular = dto.CardHolderAliasSingular ?? tenant.CardHolderAliasSingular;
        tenant.CardHolderAliasPlural = dto.CardHolderAliasPlural ?? tenant.CardHolderAliasPlural;
        tenant.CardHolderUniqueIdentifierAlias = dto.CardHolderUniqueIdentifierAlias ?? tenant.CardHolderUniqueIdentifierAlias;
        tenant.HelpPortalUrl = dto.HelpPortalUrl ?? tenant.HelpPortalUrl;
        tenant.Logo = dto.Logo ?? tenant.Logo;
        tenant.LogoMimeType = dto.LogoMimeType ?? tenant.LogoMimeType;

        return tenant;
    }
}

internal sealed class UpdateTenantMapper : IMapsToEntity<UpdateTenantDTO, Tenant>
{
    public static Tenant ToEntity(UpdateTenantDTO dto)
    {
        var tenant = new Tenant
        {
            Name = dto.Name,
            AllowLockUpdates = dto.AllowLockUpdates,
        };

        tenant.CardHolderAliasSingular = dto.CardHolderAliasSingular ?? tenant.CardHolderAliasSingular;
        tenant.CardHolderAliasPlural = dto.CardHolderAliasPlural ?? tenant.CardHolderAliasPlural;
        tenant.CardHolderUniqueIdentifierAlias = dto.CardHolderUniqueIdentifierAlias ?? tenant.CardHolderUniqueIdentifierAlias;
        tenant.HelpPortalUrl = dto.HelpPortalUrl ?? tenant.HelpPortalUrl;
        tenant.Logo = dto.Logo ?? tenant.Logo;
        tenant.LogoMimeType = dto.LogoMimeType ?? tenant.LogoMimeType;

        return tenant;
    }
}
