using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class CardHolderAndCardCredentialsMapper : IMapsToDTO<CardHolderAndCardCredentialsDTO, CardHolder>
{
    public static CardHolderAndCardCredentialsDTO ToDTO(CardHolder entity) => new
    (
        CardHolder: CardHolderMapper.ToDTO(entity),
        CardCredentials: entity.CardCredentials?.Select(CardCredentialMapper.ToDTO).ToList()
    );
}