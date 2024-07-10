using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Domain;
using System.Text.RegularExpressions;

namespace STSL.SmartLocker.Utils.Data.Services.Validators;

internal sealed partial class CardHolderValidator : IValidator<CardHolder>
{
    public IValidationResult Validate(CardHolder value)
    {
        var result = new ValidationResult();

        if (value.FirstName.Length > 256 || value.FirstName.Length < 2)
        {
            result.AddError(nameof(value.FirstName), "Must be between 2 and 256 characters");
        }
        if (value.LastName.Length > 256 || value.LastName.Length < 2)
        {
            result.AddError(nameof(value.LastName), "Must be between 2 and 256 characters");
        }
        if (!string.IsNullOrWhiteSpace(value.Email) && !IsValidEmail(value.Email))
        {
            result.AddError(nameof(value.Email), "Must be a valid email address");
        }
        if (value.UniqueIdentifier != string.Empty && value.UniqueIdentifier is not null && value.UniqueIdentifier.Length > 256)
        {
            result.AddError(nameof(value.UniqueIdentifier), "Must be 256 characters or less");
        }

        return result;
    }

    private static bool IsValidEmail(string email)
        => EmailRegex().IsMatch(email);

    [GeneratedRegex("^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$")]
    private static partial Regex EmailRegex();
}
