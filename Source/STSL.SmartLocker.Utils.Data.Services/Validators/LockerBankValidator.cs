using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class LockerBankValidator : IValidator<LockerBank>
{
    public IValidationResult Validate(LockerBank value)
    {
        var result = new ValidationResult();

        if (value.LocationId == Guid.Empty)
        {
            result.AddError(nameof(value.LocationId), "Cannot be an empty Id");
        }
        if (string.IsNullOrWhiteSpace(value.Name) || value.Name.Length > 256 || value.Name.Length < 2)
        {
            result.AddError(nameof(value.Name), "Must be between 2 and 256 characters");
        }
        if (value.Description.Length > 256)
        {
            result.AddError(nameof(value.Description), "Must be 256 characters or less");
        }
        if (!Enum.IsDefined(value.Behaviour))
        {
            result.AddError(nameof(value.Behaviour), "Must be a valid behaviour");
        }

        return result;
    }
}