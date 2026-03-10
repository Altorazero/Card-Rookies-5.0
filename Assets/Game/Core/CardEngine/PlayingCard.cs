public interface IPlayingCard
{
    public Geid Id { get; }
    public string Name { get; }
    public string Description { get; }
    // public CardType Type { get; }
    // public int ManaCost { get; }
    public CardGraphNode CardGraphRootNode { get; }
    /*    public PlayingCard(string name, string description, *//*CardType type,*//* int manaCost, CardGraphNode cardGraphRootNode)
        {
            Id = Geid.New;
            Name = name;
            Description = description;
            Type = type;
            ManaCost = manaCost;
            CardGraphRootNode = cardGraphRootNode;
        }*/
}

public class BasicPlayingCard : IPlayingCard
{
    public Geid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    // public CardType Type { get; private set; }
    // public int ManaCost { get; private set; }
    public CardGraphNode CardGraphRootNode { get; private set; }
    public BasicPlayingCard(string name, string description, /*CardType type,*/ int manaCost, CardGraphNode cardGraphRootNode)
    {
        Id = Geid.New;
        Name = name;
        Description = description;
        // Type = type;
        // ManaCost = manaCost;
        CardGraphRootNode = cardGraphRootNode;
    }
}