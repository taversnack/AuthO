using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

namespace STSL.SmartLocker.Utils.Data.Services.Helpers;

internal sealed record ValidationError(string PropertyName, string Description) : IValidationError;

internal sealed class ValidationResult : IValidationResult
{
    public bool IsValid { get; private set; } = true;

    private readonly List<ValidationError> _validationErrors = new();
    public IReadOnlyCollection<IValidationError> Errors { get => _validationErrors; }

    public ValidationResult() { }
    public ValidationResult(List<ValidationError> errors) => (_validationErrors, IsValid) = (errors, false);
    public void AddError(ValidationError error)
    {
        _validationErrors.Add(error);
        IsValid = false;
    }
    public void AddError(string propertyName, string description) => AddError(new(propertyName, description));

    // TODO: Refactor this into it's own type with Add(IValidationError) and get => ErrorsDictionary
    /// <summary>
    /// This is a helper method that just creates an empty dictionary for numbered error results e.g.<br/>
    /// 1: "name": "too short", "description": "too long"<br/>
    /// 2: "name": "too long", "something": "too vague"<br/>
    /// </summary>
    /// <returns></returns>
    internal static Dictionary<int, IReadOnlyDictionary<string, string>> CreateIndexedResults() => new();
}