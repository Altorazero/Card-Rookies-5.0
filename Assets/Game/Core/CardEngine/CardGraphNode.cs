using System.Collections.Generic;

public class CardGraphNode
{
    public Geid Id { get; private set; }
    public IEnumerable<IGameEvent> Events { get; private set; } = new List<IGameEvent>();
    private readonly List<(CardGraphNode, IPredicate)> tiedNodesList = new List<(CardGraphNode, IPredicate)>();
    public IEnumerable<(CardGraphNode, IPredicate)> TiedNodes => tiedNodesList;

    public CardGraphNode(IEnumerable<IGameEvent> gameEvents)
    {
        Events = gameEvents;
        Id = Geid.New;
    }
    public void TieNode(CardGraphNode node, IPredicate condition)
    {
        tiedNodesList.Add((node, condition));
    }
}