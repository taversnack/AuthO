using System.Runtime.InteropServices;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.CLI.Models;

// Abstract type that will be used to read / write common config options
internal sealed class LockerConfig
{
    public LockerSerial Id { get; set; }
    public LockerCards Cards { get; init; } = new LockerCards();
    public LockerSecurityMode? SecurityMode { get; set; } = null;

    public static LockerConfigJson ConvertToEndpointFormat(LockerConfig config, bool roundUpCardConfigBlocksWithEmptyValues = true)
    {
        var convertedConfig = new LockerConfigJson
        {
            Address = config.Id
        };

        if (config.SecurityMode is not null)
        {
            convertedConfig.Property.LockerSecurityMode = new ReadWriteLockerProperty<LockerSecurityMode?>(config.SecurityMode);
        }
        if (config.Cards is not null)
        {
            var cards = CollectionsMarshal.AsSpan((List<Card>)config.Cards.Value);
            var cardsLength = cards.Length;

            var requiredBlocks = (cardsLength + Constants.LockerConfigCardBlockMaxCount - 1) / Constants.LockerConfigCardBlockMaxCount;

            for (int card = 0, block = 0; block < requiredBlocks; block++)
            {
                var requiredLeft = cardsLength - (block * Constants.LockerConfigCardBlockMaxCount);
                requiredLeft = requiredLeft > Constants.LockerConfigCardBlockMaxCount ? Constants.LockerConfigCardBlockMaxCount : requiredLeft;
                var cardValue = new object[roundUpCardConfigBlocksWithEmptyValues ? Constants.LockerConfigCardBlockMaxCount : requiredLeft][];

                for (var i = 0; i < Constants.LockerConfigCardBlockMaxCount && card < cardsLength; i++, card++)
                {
                    cardValue[i] = new object[3];
                    cardValue[i][0] = cards[card].Id;
                    cardValue[i][1] = (int)cards[card].Type;
                    cardValue[i][2] = 0;
                }

                if (roundUpCardConfigBlocksWithEmptyValues && requiredLeft < Constants.LockerConfigCardBlockMaxCount)
                {
                    for (var i = requiredLeft; i < Constants.LockerConfigCardBlockMaxCount; i++)
                    {
                        cardValue[i] = new object[3];
                        cardValue[i][0] = Constants.EmptyCardString;
                        cardValue[i][1] = 0;
                        cardValue[i][2] = 0;
                    }
                }

                var cardProperty = new ReadWriteLockerProperty<object[][]>(cardValue);

                // Without reflection:
                switch (block)
                {
                    case 0: convertedConfig.Property.Cards0 = cardProperty; break;
                    case 1: convertedConfig.Property.Cards1 = cardProperty; break;
                    case 2: convertedConfig.Property.Cards2 = cardProperty; break;
                    case 3: convertedConfig.Property.Cards3 = cardProperty; break;
                    case 4: convertedConfig.Property.Cards4 = cardProperty; break;
                    case 5: convertedConfig.Property.Cards5 = cardProperty; break;
                    case 6: convertedConfig.Property.Cards6 = cardProperty; break;
                    case 7: convertedConfig.Property.Cards7 = cardProperty; break;
                    case 8: convertedConfig.Property.Cards8 = cardProperty; break;
                    case 9: convertedConfig.Property.Cards9 = cardProperty; break;
                    case 10: convertedConfig.Property.Cards10 = cardProperty; break;
                    case 11: convertedConfig.Property.Cards11 = cardProperty; break;
                    case 12: convertedConfig.Property.Cards12 = cardProperty; break;
                    case 13: convertedConfig.Property.Cards13 = cardProperty; break;
                    case 14: convertedConfig.Property.Cards14 = cardProperty; break;
                    case 15: convertedConfig.Property.Cards15 = cardProperty; break;
                    case 16: convertedConfig.Property.Cards16 = cardProperty; break;
                    case 17: convertedConfig.Property.Cards17 = cardProperty; break;
                    case 18: convertedConfig.Property.Cards18 = cardProperty; break;
                    case 19: convertedConfig.Property.Cards19 = cardProperty; break;
                }

                // With reflection:
                //PropertyInfo cardProperty = typeof(Property).GetProperty($"Cards{block}", BindingFlags.Public | BindingFlags.Instance);
                //cardProperty.SetValue(convertedConfig.Property, new ReadWriteLockerProperty<object[][]>(cardValue));

            }
        }

        return convertedConfig;
    }
}

internal sealed class LockersSetup
{
    public IList<LockerConfig> Lockers { get; set; } = new List<LockerConfig>();
}

internal static class LockerConfigJsonExentions
{
    private static IEnumerable<Card> FilterCards(object[][] cards) => cards
        .Where(card =>
        {
            // JsonDeserializer sees object and creates JsonElement;
            // therefore we check if the object[x][1] is a JsonElement type
            // as JsonElement cannot be cast directly in the same way object can
            // (object is reference type, JsonElement is a value type wrapper)
            if (card is null || card[0] is null || card[1] is null)
            {
                throw new ArgumentNullException(nameof(card), "Card or one of it's properties is null!");
            }
            CardType cardType = (CardType)(card[1] is JsonElement cardTypeUnderlying ? cardTypeUnderlying.GetByte() : card[1]);
            return Enum.IsDefined(cardType) && cardType != CardType.Unregistered;
        })
        .Select(card => new Card(card[0].ToString()!, (CardType)((JsonElement)card[1]).GetByte()));

    public static LockerConfig ConvertToSimplifiedFormat(this LockerConfigJson endpointFormat) => new()
    {
        Id = endpointFormat.Address.GetValueOrDefault(),
        SecurityMode = endpointFormat.Property.LockerSecurityMode?.Value,
        Cards = new LockerCards(
            FilterCards(
                new[] {
                    endpointFormat.Property.Cards0?.Value,
                    endpointFormat.Property.Cards1?.Value,
                    endpointFormat.Property.Cards2?.Value,
                    endpointFormat.Property.Cards3?.Value,
                    endpointFormat.Property.Cards4?.Value,
                    endpointFormat.Property.Cards5?.Value,
                    endpointFormat.Property.Cards6?.Value,
                    endpointFormat.Property.Cards7?.Value,
                    endpointFormat.Property.Cards8?.Value,
                    endpointFormat.Property.Cards9?.Value,
                    endpointFormat.Property.Cards10?.Value,
                    endpointFormat.Property.Cards11?.Value,
                    endpointFormat.Property.Cards12?.Value,
                    endpointFormat.Property.Cards13?.Value,
                    endpointFormat.Property.Cards14?.Value,
                    endpointFormat.Property.Cards15?.Value,
                    endpointFormat.Property.Cards16?.Value,
                    endpointFormat.Property.Cards17?.Value,
                    endpointFormat.Property.Cards18?.Value,
                    endpointFormat.Property.Cards19?.Value,
                }.OfType<object[][]>().SelectMany(x => x).ToArray()
            )
        )
    };
}