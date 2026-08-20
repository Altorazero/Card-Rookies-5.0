using System.Collections.Generic;

public class PlayCardEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    public GEID SystemSourceId { get; }

    public CardInstance Card;
    public IEntity Caster;
    
    public CardExecution Execution;
    public EventScratch Scratch { get; set; }

    public PlayCardEvent(GEID systemSourceId, CardInstance card, IEntity caster)
    {
        SystemSourceId = systemSourceId;
        Card = card;
        Caster = caster;
    }
}
