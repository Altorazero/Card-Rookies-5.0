using System;
using System.Collections.Generic;

[Serializable]
public class HandComponent : CardStorageBase
{
    public HandComponent() : base() { }
    public HandComponent(IEnumerable<CardInstance> cards) : base(cards) { }

    public void AddToHand(CardInstance card)
    {
        Add(card);
    }
    
    public bool HasDrawnInitial { get; set; } = false;
    public int AutoDrawCount { get; set; } = 1;
    
    public void AutoDraw(DeckComponent deck)
    {
        for (int i = 0; i < AutoDrawCount; i++)
        {
            var card = deck.Draw();
            if (card != null)
                Add(card);
        }
    }

    public void DrawMultiple(DeckComponent deck, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var card = deck.Draw();
            if (card != null)
                Add(card);
        }
    }
}
