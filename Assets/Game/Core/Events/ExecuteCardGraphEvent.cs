public class ExecuteCardGraphEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public CardGraphNode CardGraphNode { get; }

    public ExecuteCardGraphEvent(Geid sourceId, CardGraphNode cardGraphNode)
    {
        Id = Geid.New;
        SystemSourceId = sourceId;
        CardGraphNode = cardGraphNode;
    }
}
