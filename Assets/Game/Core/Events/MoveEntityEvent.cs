using System.Collections.Generic;

public class MoveEntityEvent : IGameEvent, IHaveSubjects
{
    public HexCoordinates NewPosition { get; }
    public EventStatus Status { get; set; }
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<Subject> SubjectsList { get; set; }
    public MoveEntityEvent(Geid systemsourceId, Geid targetId, HexCoordinates newPosition)
    {
        Id = Geid.New;
        Status = EventStatus.Pending;
        SystemSourceId = systemsourceId;
        NewPosition = newPosition;
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = targetId, Role = SubjectRole.Target }
        };
    }
}