using System.Collections.Generic;

public class MoveEntityEvent : IGameEvent, IHaveSubjects
{
    public HexCoordinates NewPosition { get; }
    public EventStatus Status { get; set; }
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    public MoveEntityEvent(Geid systemSourceId, Geid targetId, HexCoordinates newPosition)
    {
        Id = Geid.New;
        Status = EventStatus.Pending;
        SystemSourceId = systemSourceId;
        NewPosition = newPosition;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Target, targetId)
        );
    }
}
