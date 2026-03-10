using System.Collections.Generic;

public class RandomMovementEvent : IGameEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<Subject> SubjectsList { get; set; }
    public int Radius { get; }
    public RandomMovementEvent(Geid systemSourceId, Geid targetId, int radius)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        Radius = radius;
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = targetId, Role = SubjectRole.Target }
        };
    }
}