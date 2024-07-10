using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class LocationValidator : IValidator<Location>
{
    public IValidationResult Validate(Location value)
    {
        var result = new ValidationResult();

        if (value.Name.Length > 256 || value.Name.Length < 2)
        {
            result.AddError(nameof(value.Name), "Must be between 2 and 256 characters");
        }
        if (value.Description.Length > 256)
        {
            result.AddError(nameof(value.Description), "Must be 256 characters or less");
        }

        return result;
    }
}
