using STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Models;
using STSL.SmartLocker.Utils.Common.Data;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.Mappings;

internal static class LockConfigMapper
{
    private const int CardBlockCapacity = 5;
    private const int TotalCardBlocks = 20;
    private const int CardsCapacity = CardBlockCapacity * TotalCardBlocks;
    private const string EmptyCardString = "0000000000000000";

    internal static LockConfig ToBlubugFormat(this UpdateLockConfigDTO dto)
    {
        LockConfig lockConfig = new();

        if (dto.UpdateProperties.HasFlag(LockConfigUpdateProperties.OperatingMode) && dto.OperatingMode is not null)
        {
            lockConfig.Property.LockOperatingMode = new((int)dto.OperatingMode);
        }
        if (dto.UpdateProperties.HasFlag(LockConfigUpdateProperties.Cards) && dto.Cards is not null)
        {
            if (dto.Cards.Count > CardsCapacity)
            {
                throw new ArgumentException($"No more than {CardsCapacity} cards allowed", nameof(dto));
            }

            lockConfig.Property.Cards ??= new();

            // TODO: Tidy and comment the below, could probably be refactored a bit!

            var requiredBlocks = dto.ReplaceAllCardBlocks ? TotalCardBlocks : (dto.Cards.Count + CardBlockCapacity - 1) / CardBlockCapacity;

            for (int block = 0, totalCardsWritten = 0; block < requiredBlocks; block++)
            {
                var cardValue = new object[CardBlockCapacity][];

                for (var blockCard = 0; blockCard < CardBlockCapacity; blockCard++, totalCardsWritten++)
                {
                    if (dto.Cards.Count > totalCardsWritten)
                    {
                        cardValue[blockCard] = dto.Cards[totalCardsWritten].ToBlubugFormat();
                    }
                    else
                    {
                        cardValue[blockCard] = EmptyCard();
                    }
                }

                var cardProperty = new ReadWriteLockProperty<object[][]>(cardValue);

                lockConfig.Property.Cards.TryAdd("Cards" + block, cardProperty);
            }
        }

        return lockConfig;
    }

    internal static LockConfigDTO ToDTO(this LockConfig lockConfig)
    {
        var operatingModeValue = lockConfig.Property.LockOperatingMode?.Value ?? 0;

        var securitySweepValue = lockConfig.Property.LockSecuritySweep?.Value ?? 0;

        // TODO: Tidy up..
        // This is terribly unsafe code. As is the below cardsPending select below.
        // Assumes that Deserialize will always succeed, everything is always in correct format etc
        // which in fairness it probably should be! Life isn't fair though, you need error handling.
        var cardsValue = lockConfig.Property.Cards?
            .Select(keyValuePair => ((JsonElement)keyValuePair.Value).Deserialize<ReadWriteLockProperty<JsonElement[][]>>())
            .SelectMany(x => x?.Value ?? Array.Empty<JsonElement[]>())
            .Select(x => x.ToDTO())
            .Where(x => x.SerialNumber != EmptyCardString)
            .ToList();

        //var cardsValue = new List<LockConfigCardDTO>();

        //if (lockConfig.Property.Cards is not null)
        //{
        //    foreach (var cardBlock in lockConfig.Property.Cards)
        //    {
        //        var cardElement = (JsonElement)cardBlock.Value;
        //        var cardProperty = cardElement.Deserialize<ReadWriteLockProperty<JsonElement[][]>>();
        //        if(cardProperty is not null && cardProperty.Value is not null)
        //        {
        //            foreach(var card in cardProperty.Value)
        //            {
        //                cardsValue.Add(card.ToDTO());
        //            }
        //        }
        //    }
        //}

        var operatingModePending = (lockConfig.Property.LockOperatingMode?.Pending ?? false) ?
            lockConfig.Property.LockOperatingMode?.Target : null;

        var securitySweepPending = (lockConfig.Property.LockSecuritySweep?.Pending ?? false) ?
            lockConfig.Property.LockSecuritySweep?.Target : null;

        var cardsPending = lockConfig.Property.Cards?
            .Select(keyValuePair => ((JsonElement)keyValuePair.Value).Deserialize<ReadWriteLockProperty<JsonElement[][]>>())
            .Where(x => x?.Pending ?? false)
            .SelectMany(x => x?.Value ?? Array.Empty<JsonElement[]>())
            .Select(x => x.ToDTO())
            .ToList();

        var hasPendingCards = cardsPending?.Any() ?? false;

        return new()
        {
            Value = new()
            {
                LockOperatingMode = (LockOperatingMode)operatingModeValue,
                LockSecuritySweep = securitySweepValue,
                Cards = cardsValue,
            },
            Pending = (operatingModePending.HasValue || securitySweepPending.HasValue || hasPendingCards) ? new()
            {
                LockOperatingMode = operatingModePending.HasValue ? (LockOperatingMode)operatingModePending : null,
                LockSecuritySweep = securitySweepPending,
                Cards = cardsPending?.Where(x => x.SerialNumber != EmptyCardString).ToList(),
            } : null
        };
    }

    internal static object[] ToBlubugFormat(this LockConfigCardDTO cardDTO)
        => new object[3] { cardDTO.SerialNumber, (int)cardDTO.CardType, 0 };

    // TODO: Add some safety to this.
    // This is unsafe; should only be called on an exactly the right format array.
    // This could throw an IndexOutOfRangeException or InvalidCastException
    //internal static LockConfigCardDTO ToDTO(this object[] card)
    //    => new(card[0]?.ToString() ?? EmptyCardString, (CardType)card[1]);

    internal static LockConfigCardDTO ToDTO(this JsonElement[] card)
        => new(card[0].GetString() ?? EmptyCardString, (CardType)card[1].GetInt32());

    internal static object[] EmptyCard()
        => new object[3] { EmptyCardString, 0, 0 };
}
