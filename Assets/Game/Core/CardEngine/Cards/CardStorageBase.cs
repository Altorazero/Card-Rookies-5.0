using System.Collections.Generic;
using System.Linq;

public abstract class CardStorageBase
{
    protected readonly List<CardInstance> _cards = new List<CardInstance>();
    
    public IReadOnlyList<CardInstance> Cards => _cards;
    public int Count => _cards.Count;

    public CardStorageBase() { }

    public CardStorageBase(IEnumerable<CardInstance> initialCards)
    {
        if (initialCards != null)
        {
            _cards.AddRange(initialCards);
        }
    }

    public void AddToTop(CardInstance card) => _cards.Insert(0, card);
    public void AddToBottom(CardInstance card) => _cards.Add(card);

    public CardInstance RemoveFromTop()
    {
        if (_cards.Count == 0) return null;
        var c = _cards[0];
        _cards.RemoveAt(0);
        return c;
    }

    public CardInstance RemoveFromBottom()
    {
        if (_cards.Count == 0) return null;
        var c = _cards[_cards.Count - 1];
        _cards.RemoveAt(_cards.Count - 1);
        return c;
    }

    public void Add(CardInstance card) => AddToBottom(card);
    
    public bool Remove(CardInstance card) 
    {
        return _cards.Remove(card);
    }
    
    public void RemoveAt(int index) => _cards.RemoveAt(index);
    
    public CardInstance FindById(GEID id) => _cards.FirstOrDefault(c => c.Id == id);
    
    public bool RemoveById(GEID id)
    {
        var card = FindById(id);
        if (card != null)
        {
            return Remove(card);
        }
        return false;
    }

    public void Shuffle(BattleRng rng)
    {
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    public bool IsEmpty()
    {
        return _cards.Count == 0;
    }
}
