namespace STSL.SmartLocker.Utils.Domain;

public class Tenant : EntityBase
{
    public required string Name { get; set; }
    public bool AllowLockUpdates { get; set; } = false;
    public string CardHolderAliasSingular { get; set; } = "Card Holder";
    public string CardHolderAliasPlural { get; set; } = "Card Holders";
    public string CardHolderUniqueIdentifierAlias { get; set; } = "Unique Identifier";
    //public required string LockerServiceTagBaseUrl { get; set; }
    public string? HelpPortalUrl { get; set; }
    public byte[]? Logo { get; set; }
    public string? LogoMimeType { get; set; }
}
