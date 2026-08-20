using System;
using System.Collections.Generic;

[Serializable]
public class DiscardComponent : CardStorageBase
{
    public DiscardComponent() : base() { }
    public DiscardComponent(IEnumerable<CardInstance> cards) : base(cards) { }
}
