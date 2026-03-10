using System.Collections.Generic;

public class RandomMovementEvent : IGameEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }
    public int Radius { get; }

    public RandomMovementEvent(Geid systemSourceId, Geid targetId, int radius)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        Radius = radius;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Target, targetId)
        );
    }
}
