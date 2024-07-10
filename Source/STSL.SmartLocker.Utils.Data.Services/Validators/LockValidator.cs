using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class LockValidator : IValidator<Lock>
{
    public IValidationResult Validate(Lock value)
    {
        var result = new ValidationResult();

        if (!Enum.IsDefined(value.OperatingMode))
        {
            result.AddError(nameof(value.OperatingMode), "Must be a valid operating mode");
        }

        return result;
    }
}
