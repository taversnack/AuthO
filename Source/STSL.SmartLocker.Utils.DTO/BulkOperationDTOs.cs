namespace STSL.SmartLocker.Utils.DTO;

public readonly struct CreateLockerAndLockDTO
{
    public CreateLockerDTO Locker { get; init; }
    public CreateLockDTO Lock { get; init; }
}

public readonly struct CreateBulkLockerAndLocksDTO
{
    public List<CreateLockerAndLockDTO> LockerAndLocks { get; init; }
}

public readonly struct CreateCardHolderAndCardCredentialDTO
{
    public CreateCardHolderDTO CardHolder { get; init; }
    public CreateCardCredentialDTO CardCredential { get; init; }
}

public readonly struct CreateBulkCardHolderAndCardCredentialsDTO
{
    public List<CreateCardHolderAndCardCredentialDTO> CardHolderAndCardCredentials { get; init; }
}

public readonly struct CreateLockerAndLockAndCardHolderAndCardCredentialDTO
{
    public CreateLockerDTO Locker { get; init; }
    public CreateLockDTO Lock { get; init; }
    public CreateCardHolderDTO CardHolder { get; init; }
    public CreateCardCredentialDTO CardCredential { get; init; }
}

public readonly struct CreateBulkLockerAndLockAndCardHolderAndCardCredentialsDTO
{
    public List<CreateLockerAndLockAndCardHolderAndCardCredentialDTO> LockerAndLockAndCardHolderAndCardCredentials { get; init; }
}

public readonly record struct MoveLockersToLockerBankDTO(Guid? Origin, Guid Destination, List<Guid> LockerIds);