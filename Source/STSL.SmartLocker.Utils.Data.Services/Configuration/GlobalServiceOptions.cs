namespace STSL.SmartLocker.Utils.Data.Services.Configuration;

public sealed class GlobalServiceOptions
{
    public bool ThrowOnFirstValidationErrorForBulkOperations { get; set; } = false;
    public bool ThrowNotFoundWhenDeletingNonExistantEntity { get; set; } = false;
    public bool EmptyBulkOperationIsError { get; set; } = false;
}
