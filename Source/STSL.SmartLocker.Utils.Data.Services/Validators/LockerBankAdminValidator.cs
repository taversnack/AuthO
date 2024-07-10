using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class LockerBankAdminValidator : IValidator<LockerBankAdmin>
{
    public IValidationResult Validate(LockerBankAdmin value)
    {
        var result = new ValidationResult();

        if (value.LockerBankId == Guid.Empty)
        {
            result.AddError(nameof(value.LockerBankId), "Cannot be an empty Id");
        }
        if (value.CardHolderId == Guid.Empty)
        {
            result.AddError(nameof(value.CardHolderId), "Cannot be an empty Id");
        }

        return result;
    }
}
