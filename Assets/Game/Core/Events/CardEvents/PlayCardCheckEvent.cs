using System.Collections.Generic;

public class PlayCardCheckEvent : IGameEvent, IGuardPhaseEvent, IAfterPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }

    public CardInstance Card { get; }

    public IEntity Caster { get; }

    public IReadOnlyList<GEID> SelectedTargets { get; }
    public EventScratch Scratch { get; set; }

    public PlayCardEvent SpawnedPlayCardEvent { get; set; }

    public Dictionary<Role, List<IEntity>> Subjects { get; set; }

    public PlayCardCheckEvent(GEID systemSourceId, CardInstance card, IEntity caster, IReadOnlyList<GEID> selectedTargets = null)
    {
        Id = GEID.New;
        SystemSourceId = systemSourceId;
        Card = card;
        Caster = caster;
        SelectedTargets = selectedTargets ?? System.Array.Empty<GEID>();
        Subjects = SubjectsHelper.Create((Role.Source, caster));
    }
}
