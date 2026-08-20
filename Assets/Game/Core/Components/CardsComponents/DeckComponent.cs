using System;
using System.Collections.Generic;

[Serializable]
public class DeckComponent : CardStorageBase
{
    public DeckComponent() : base() { }
    public DeckComponent(IEnumerable<CardInstance> cards) : base(cards) { }

    public CardInstance Draw()
    {
        return RemoveFromTop();
    }
}
