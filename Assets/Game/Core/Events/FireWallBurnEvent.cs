using System.Collections.Generic;

/// <summary>
/// Событие массового наложения горения через таргетинг.
/// Применяется системой FireWallBurnSystem.
/// </summary>
public class FireWallBurnEvent : IGameEvent, ITargetResolvePhaseEvent, IApplyPhaseEvent, INeedTargeting, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }
    public ITargetingSpec TargetingSpec { get; set; }

    public int DamagePerTick { get; }
    public int Ticks { get; }

    public FireWallBurnEvent(Geid systemSourceId, Geid sourceId, ITargetingSpec targetingSpec, int damagePerTick, int ticks)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        TargetingSpec = targetingSpec;
        DamagePerTick = damagePerTick;
        Ticks = ticks;
        Subjects = SubjectsHelper.Create((SubjectRole.Source, sourceId));
    }
}
