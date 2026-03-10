using System.Collections.Generic;

public class MassDamageEvent : IGameEvent, IGuardPhaseEvent, IModifyPhaseEvent, IApplyPhaseEvent, IAfterPhaseEvent, ITargetResolvePhaseEvent, INeedTargeting
{
    public EventStatus Status { get; set; }
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public int DamageAmount { get; set; }
    public int Amount
    {
        get => DamageAmount;
        set => DamageAmount = value;
    }
    public DamageType DamageType { get; }
    public List<Subject> SubjectsList { get; set; }

    public ITargetingSpec TargetingSpec { get; set; }
    public MassDamageEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int damageAmount, ITargetingSpec targetingSpec, DamageType damageType = DamageType.Physical)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        DamageAmount = damageAmount;
        DamageType = damageType;
        TargetingSpec = targetingSpec;
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = targetId, Role = SubjectRole.Target },
            new Subject { Entity = sourceId, Role = SubjectRole.Source }
        };
    }
}

public enum DamageType
{
    Physical,
    Magical,
    True
}