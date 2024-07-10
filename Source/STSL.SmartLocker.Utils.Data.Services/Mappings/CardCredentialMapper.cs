using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class CardCredentialMapper : IMapsToDTO<CardCredentialDTO, CardCredential>
{
    public static CardCredentialDTO ToDTO(CardCredential entity) => new
    (
        Id: entity.Id,
        SerialNumber: entity.SerialNumber,
        HidNumber: entity.HidNumber,
        CardType: entity.CardType,
        CardLabel: entity.CardLabel,
        CardHolderId: entity.CardHolderId
    );
}

internal sealed class CreateCardCredentialMapper : IMapsToEntity<CreateCardCredentialDTO, CardCredential>
{
    public static CardCredential ToEntity(CreateCardCredentialDTO dto)
    {
        var cardCredential = new CardCredential
        {
            SerialNumber = dto.SerialNumber,
            HidNumber = dto.HidNumber,
            CardType = dto.CardType,
            CardLabel = dto.CardLabel,
        };

        cardCredential.CardHolderId = dto.CardHolderId ?? cardCredential.CardHolderId;

        return cardCredential;
    }
}

internal sealed class UpdateCardCredentialMapper : IMapsToEntity<UpdateCardCredentialDTO, CardCredential>
{
    public static CardCredential ToEntity(UpdateCardCredentialDTO dto)
    {
        var cardCredential = new CardCredential
        {
            SerialNumber = dto.SerialNumber,
            HidNumber = dto.HidNumber,
            CardType = dto.CardType,
            CardLabel = dto.CardLabel,
        };

        cardCredential.CardHolderId = dto.CardHolderId ?? cardCredential.CardHolderId;

        return cardCredential;
    }
}
