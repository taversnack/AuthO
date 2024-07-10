namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

public interface IValidationError
{
    string PropertyName { get; }
    string Description { get; }
}

public interface IValidationResult
{
    bool IsValid { get; }
    bool IsInvalid => !IsValid;

    IReadOnlyCollection<IValidationError> Errors { get; }
    // This version can only have one error per property
    //IReadOnlyDictionary<string, string> ErrorsDictionary
    //    => Errors.ToDictionary(x => x.PropertyName, x => x.Description);

    // This version concatenates multiple error descriptions for the same property name
    IReadOnlyDictionary<string, string> ErrorsDictionary
        => Errors.GroupBy(
            x => x.PropertyName,
            x => x.Description,
            (propertyName, descriptions) => (propertyName, description: string.Join(' ', descriptions))
        ).ToDictionary(x => x.propertyName, x => x.description);
}

public interface IValidator<T>
{
    IValidationResult Validate(T value);
}
