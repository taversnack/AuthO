namespace STSL.SmartLocker.Utils.DTO;

// TODO: Benchmark API query performance on class vs readonly struct especially for small
// structs like this. Dereferencing takes time and for lists especially it's far less cache friendly
public sealed record CreateTenantDTO(
    string Name,
    string? CardHolderAliasSingular = null,
    string? CardHolderAliasPlural = null,
    string? CardHolderUniqueIdentifierAlias = null,
    string? HelpPortalUrl = null,
    byte[]? Logo = null,
    string? LogoMimeType = null,
    bool AllowLockUpdates = false);

public sealed record TenantDTO(
    Guid Id,
    string Name,
    string? CardHolderAliasSingular = null,
    string? CardHolderAliasPlural = null,
    string? CardHolderUniqueIdentifierAlias = null,
    string? HelpPortalUrl = null,
    byte[]? Logo = null,
    string? LogoMimeType = null,
    bool AllowLockUpdates = false);

public sealed record UpdateTenantDTO(
    string Name,
    string? CardHolderAliasSingular = null,
    string? CardHolderAliasPlural = null,
    string? CardHolderUniqueIdentifierAlias = null,
    string? HelpPortalUrl = null,
    byte[]? Logo = null,
    string? LogoMimeType = null,
    bool AllowLockUpdates = false);
