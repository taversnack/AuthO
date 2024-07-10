namespace STSL.SmartLocker.Utils.Domain;

public interface IEntityReferenceImage
{
    public ImageMetaData MetaData { get; set; }
    public Guid? EntityId { get; set; }
}

public interface IHasReferenceImage<T> where T : IUsesGuidId, IEntityReferenceImage
{
    public Guid? CurrentReferenceImageId { get; set; }
    T? CurrentReferenceImage { get; set; }
    List<T>? ReferenceImages { get; set; }
}

public sealed class ImageMetaData
{
    public required string FileName { get; set; }
    public string? AzureBlobName { get; set; }
    public required int PixelWidth { get; set; }
    public required int PixelHeight { get; set; }
    public required int ByteSize { get; set; }
    public required string MimeType { get; set; }
    public DateTimeOffset UploadedDate { get; set; } = DateTimeOffset.UtcNow;
    public string? UploadedByCardHolderEmail { get; set; }
}

public sealed class LocationReferenceImage : EntityBaseWithTenant, IUsesGuidId, IEntityReferenceImage
{
    public Guid Id { get; set; }
    public Guid? LocationId { get; set; }
    public required ImageMetaData MetaData { get; set; }
    public Guid? EntityId { get => LocationId; set => LocationId = value; }
}

public sealed class LockerBankReferenceImage : EntityBaseWithTenant, IUsesGuidId, IEntityReferenceImage
{
    public Guid Id { get; set; }
    public Guid? LockerBankId { get; set; }
    public required ImageMetaData MetaData { get; set; }
    public Guid? EntityId { get => LockerBankId; set => LockerBankId = value; }
}

