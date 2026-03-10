using System.Collections.Generic;

public class SingleDamageEvent : IGameEvent, IGuardPhaseEvent, IModifyPhaseEvent, IApplyPhaseEvent, IAfterPhaseEvent, IHaveSubjects
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
    public List<List<Geid>> Subjects { get; set; }

    public SingleDamageEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int damageAmount, DamageType damageType = DamageType.Physical)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        DamageAmount = damageAmount;
        DamageType = damageType;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
