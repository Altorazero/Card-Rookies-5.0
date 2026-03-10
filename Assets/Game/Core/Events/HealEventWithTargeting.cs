using System.Collections.Generic;

/// <summary>
/// Событие лечения с динамическим выбором цели.
/// </summary>
public class HealEventWithTargeting : IGameEvent, IHaveSubjects, IGuardPhaseEvent, IModifyPhaseEvent, IApplyPhaseEvent, IAfterPhaseEvent, ITargetResolvePhaseEvent, INeedTargeting
{
    public int HealAmount { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }
    public ITargetingSpec TargetingSpec { get; set; }

    public HealEventWithTargeting(Geid systemSourceId, Geid sourceId, Geid targetId, int healAmount, ITargetingSpec targetingSpec)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        HealAmount = healAmount;
        TargetingSpec = targetingSpec;

        Subjects = SubjectsHelper.Empty();
        Subjects[(int)SubjectRole.Source].Add(sourceId);

        // Если targetId не пустой — добавляем как предустановленную цель
        if (!targetId.Equals(Geid.Empty))
            Subjects[(int)SubjectRole.Target].Add(targetId);
    }
}
