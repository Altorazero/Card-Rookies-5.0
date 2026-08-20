using System.Collections.Generic;

public class HealEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    public GEID SystemSourceId { get; }

    public int Amount;

    public Dictionary<Role, List<IEntity>> Subjects { get; set; }

    public EventScratch Scratch { get; set; }

    public HealEvent(GEID systemSourceId, int amount)
    {
        SystemSourceId = systemSourceId;
        Amount = amount;
        Subjects = SubjectsHelper.Empty();
    }
}
