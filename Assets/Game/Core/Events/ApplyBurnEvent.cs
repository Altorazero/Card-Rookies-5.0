using System.Collections.Generic;

/// <summary>
/// Событие применения эффекта горения к сущности.
/// Накладывает BurnComponent (или усиливает существующий) на целевую сущность.
/// </summary>
public class ApplyBurnEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    /// <summary>Урон от горения за один тик.</summary>
    public int DamagePerTick { get; }

    /// <summary>Количество тиков горения.</summary>
    public int Ticks { get; }

    public ApplyBurnEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int damagePerTick, int ticks)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        DamagePerTick = damagePerTick;
        Ticks = ticks;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
