namespace STSL.SmartLocker.Utils.DTO;

public readonly struct CardCredentialAndCardHolderDTO
{
    public CardHolderDTO? CardHolder { get; init; }
    public CardCredentialDTO CardCredential { get; init; }
}

public readonly record struct CardHolderAndCardCredentialsDTO(CardHolderDTO CardHolder, List<CardCredentialDTO>? CardCredentials = null);

public readonly record struct UpdateManyCardCredentialAndCardHoldersDTO(List<Guid>? CardHolderIds = null, List<Guid>? CardCredentialIds = null);