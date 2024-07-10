using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class CardHolderMapper : IMapsToDTO<CardHolderDTO, CardHolder>
{
    public static CardHolderDTO ToDTO(CardHolder entity) => new
    (
        Id: entity.Id,
        FirstName: entity.FirstName,
        LastName: entity.LastName,
        Email: entity.Email,
        UniqueIdentifier: entity.UniqueIdentifier,
        IsVerified: entity.IsVerified
    );
}

internal sealed class CreateCardHolderMapper : IMapsToEntity<CreateCardHolderDTO, CardHolder>
{
    public static CardHolder ToEntity(CreateCardHolderDTO dto)
    {
        var cardHolder = new CardHolder
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
        };

        cardHolder.Email = dto.Email ?? cardHolder.Email;
        cardHolder.UniqueIdentifier = dto.UniqueIdentifier ?? cardHolder.UniqueIdentifier;
        cardHolder.IsVerified = dto.IsVerified ?? cardHolder.IsVerified;

        return cardHolder;
    }
}

internal sealed class UpdateCardHolderMapper : IMapsToEntity<UpdateCardHolderDTO, CardHolder>
{
    public static CardHolder ToEntity(UpdateCardHolderDTO dto)
    {
        var cardHolder = new CardHolder
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
        };

        cardHolder.Email = dto.Email ?? cardHolder.Email;
        cardHolder.UniqueIdentifier = dto.UniqueIdentifier ?? cardHolder.UniqueIdentifier;
        cardHolder.IsVerified = dto.IsVerified ?? cardHolder.IsVerified;

        return cardHolder;
    }
}
