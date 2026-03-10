using System.Collections.Generic;

public class RandomDamageEvent : IGameEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    public int LowerBound { get; }
    public int UpperBound { get; }

    public RandomDamageEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int lowerBound, int upperBound)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
