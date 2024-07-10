using System.Collections;

namespace STSL.SmartLocker.Utils.CLI.Models;

internal enum CardType : byte
{
    Unregistered = 0,
    User = 1,
    Welcome = 2,
    Master = 3,
    SecuritySweep = 4,
}

internal class LockerCards : IList<Card>, IEnumerable<Card>
{
    public const int MaxSize = 100;
    //const int MaxUserCards = 80;
    private const int InitialCapacity = 4;

    private readonly List<Card> _value;
    // Note this isn't technically readonly as it can simply be cast back to a list
    // but it does the job unless you're cheating :P
    public IReadOnlyList<Card> Value { get => _value; }

    public int Count => ((ICollection<Card>)_value).Count;

    public bool IsReadOnly => ((ICollection<Card>)_value).IsReadOnly;

    public Card this[int index] { get => ((IList<Card>)_value)[index]; set => ((IList<Card>)_value)[index] = value; }

    public LockerCards()
    {
        _value = new List<Card>();
    }

    public LockerCards(int cardsCapacity = InitialCapacity)
    {
        _value = new List<Card>(cardsCapacity);
    }

    public LockerCards(IEnumerable<Card> cards)
    {
        var cardsCount = cards.TryGetNonEnumeratedCount(out var cardCount) ? cardCount : cards.Count();
        if (cardsCount > MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(cards), $"IEnumerable count must be no greater than {MaxSize}");
        }
        _value = new List<Card>(cards);
    }

    public bool Push(Card card)
    {
        if (Value.Count <= MaxSize)
        {
            _value.Add(card);
            return true;
        }
        return false;
    }

    public bool TryAddCards(ICollection<Card> cards)
    {
        if (cards.Count + _value.Count <= MaxSize)
        {
            _value.AddRange(cards);
            return true;
        }
        return false;
    }

    public bool RemoveCard(Card card)
    {
        return _value.Remove(card);
    }

    public Card? Pop()
    {
        var cardsCount = _value.TryGetNonEnumeratedCount(out int count) ? count : _value.Count;
        if (cardsCount < 1)
        {
            return null;
        }
        var last = _value[^1];
        _value.Remove(last);
        return last;
    }

    public IEnumerator<Card> GetEnumerator() => _value.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _value.GetEnumerator();

    public int IndexOf(Card item)
    {
        return ((IList<Card>)_value).IndexOf(item);
    }

    public void Insert(int index, Card item)
    {
        ((IList<Card>)_value).Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        ((IList<Card>)_value).RemoveAt(index);
    }

    public void Add(Card item)
    {
        ((ICollection<Card>)_value).Add(item);
    }

    public void Clear()
    {
        ((ICollection<Card>)_value).Clear();
    }

    public bool Contains(Card item)
    {
        return ((ICollection<Card>)_value).Contains(item);
    }

    public void CopyTo(Card[] array, int arrayIndex)
    {
        ((ICollection<Card>)_value).CopyTo(array, arrayIndex);
    }

    public bool Remove(Card item)
    {
        return ((ICollection<Card>)_value).Remove(item);
    }
}

// Most abstracted type which CLI end user will work with
internal record Card(CardSerial Id, CardType Type = CardType.User);