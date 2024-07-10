using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed class CardCredentialValidator : IValidator<CardCredential>
{
    public IValidationResult Validate(CardCredential value)
    {
        var result = new ValidationResult();

        if (value.SerialNumber is not null && IsValidCardSerial(value.SerialNumber))
        {
            result.AddError(nameof(value.SerialNumber), "Must be 16 hexadecimal characters");
        }
        if (string.IsNullOrWhiteSpace(value.HidNumber) || !int.TryParse(value.HidNumber, out _))
        {
            result.AddError(nameof(value.HidNumber), "Must be a numeric string");
        }
        if (!Enum.IsDefined(value.CardType))
        {
            result.AddError(nameof(value.CardType), "Must be a valid CardType");
        }
        if (value.CardLabel is not null && value.CardLabel.Length > 256)
        {
            result.AddError(nameof(value.CardType), "Must be 256 characters or less");
        }

        return result;
    }

    private bool IsValidCardSerial(string serial)
    => serial.Length == 16 && serial.IsValidHexadecimal();
}
