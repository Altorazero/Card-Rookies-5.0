using System.Collections.Generic;

public class RandomDamageEvent : IGameEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<Subject> SubjectsList { get; set; }

    public int LowerBond { get; }
    public int UpperBond { get; }
    public RandomDamageEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int lowreBond, int upperBond)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        LowerBond = lowreBond;
        UpperBond = upperBond;
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = sourceId, Role = SubjectRole.Source },
            new Subject { Entity = targetId, Role = SubjectRole.Target }
        };
    }

}