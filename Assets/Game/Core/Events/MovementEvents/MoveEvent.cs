using System.Collections.Generic;

public class MoveEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; } = GEID.New;
    public GEID SystemSourceId { get; }

    public HexCoordinates NewPosition;
    public EventScratch Scratch { get; set; }

    public Dictionary<Role, List<IEntity>> Subjects { get; set; }

    public MoveEvent(GEID systemSourceId, HexCoordinates newPosition)
    {
        SystemSourceId = systemSourceId;
        NewPosition = newPosition;
        Subjects = SubjectsHelper.Empty();
    }
}
