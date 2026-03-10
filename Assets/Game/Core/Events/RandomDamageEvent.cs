using System.Collections.Generic;

public class RandomDamageEvent : IGameEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    public int LowerBond { get; }
    public int UpperBond { get; }

    public RandomDamageEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int lowerBond, int upperBond)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        LowerBond = lowerBond;
        UpperBond = upperBond;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
