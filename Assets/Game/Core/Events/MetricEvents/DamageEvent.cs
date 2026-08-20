using System.Collections.Generic;

public class DamageEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    public GEID SystemSourceId { get; }

    public int Amount;
    public DamageType Type;

    public Dictionary<Role, List<IEntity>> Subjects { get; set; }

    public EventScratch Scratch { get; set; }

    public DamageEvent(GEID systemSourceId, IEntity source, IEntity target, int amount, DamageType type = DamageType.Physical)
    {
        SystemSourceId = systemSourceId;
        Amount = amount;
        Type = type;
        Subjects = SubjectsHelper.Empty();
        Subjects.Add(Role.Source, new List<IEntity>() { source });
        Subjects.Add(Role.Target, new List<IEntity>() { target });

    }
}
