using System.Collections.Generic;

public class CardInstance : BaseEntity
{
    public CardDefinition Definition { get; }

    public string Name => Definition.CardName;
    public string Description => Definition.Description;

    public CardInstance(CardDefinition def) : base()
    {
        Definition = def;
    }
}
