using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class LockerValidator : IValidator<Locker>
{
    public IValidationResult Validate(Locker value)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(value.Label) || value.Label.Length > 256)
        {
            result.AddError(nameof(value.Label), "Must be 256 characters or less and not empty");
        }
        if ((value.SecurityType == LockerSecurityType.SmartLock && string.IsNullOrWhiteSpace(value.ServiceTag)) || value.ServiceTag?.Length > 32)
        {
            result.AddError(nameof(value.ServiceTag), "Must be 32 characters or less and is required for a smart locker");
        }

        return result;
    }
}
