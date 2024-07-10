namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateCardHolderDTO(string FirstName, string LastName, string? Email, string? UniqueIdentifier, bool? IsVerified);
public sealed record CardHolderDTO(Guid Id, string FirstName, string LastName, string? Email, string? UniqueIdentifier, bool IsVerified);
public sealed record UpdateCardHolderDTO(string FirstName, string LastName, string? Email, string? UniqueIdentifier, bool? IsVerified);