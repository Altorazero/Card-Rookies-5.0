using System.Collections.Generic;

public class ExecuteCardGraphEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public IEnumerable<Subject> PartyList { get; set; }
    public CardGraphNode CardGraphNode { get; }
    public ExecuteCardGraphEvent(Geid sourceId, CardGraphNode cardGraphNode)
    {
        Id = Geid.New;
        SystemSourceId = sourceId;
        CardGraphNode = cardGraphNode;
        PartyList = new List<Subject>();
    }

}