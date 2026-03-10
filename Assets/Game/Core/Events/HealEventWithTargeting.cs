using System.Collections.Generic;

/// <summary>
/// Событие исцеления с поддержкой системы таргетинга
/// </summary>
public class HealEventWithTargeting : IGameEvent, IHaveSubjects, IGuardPhaseEvent, IModifyPhaseEvent, IApplyPhaseEvent, IAfterPhaseEvent, ITargetResolvePhaseEvent, INeedTargeting
{
    public int HealAmount { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<Subject> SubjectsList { get; set; }
    public ITargetingSpec TargetingSpec { get; set; }

    public HealEventWithTargeting(Geid systemSourceId, Geid sourceId, Geid targetId, int healAmount, ITargetingSpec targetingSpec)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        HealAmount = healAmount;
        TargetingSpec = targetingSpec;
        
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = sourceId, Role = SubjectRole.Source }
        };
        
        // Если targetId не пустой, добавляем его как начальную цель
        // (будет перезаписан системой таргетинга)
        if (!targetId.Equals(Geid.Empty))
        {
            SubjectsList.Add(new Subject { Entity = targetId, Role = SubjectRole.Target });
        }
    }
}