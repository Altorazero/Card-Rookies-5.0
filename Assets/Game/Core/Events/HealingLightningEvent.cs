using System.Collections.Generic;

/// <summary>
/// Событие исцеляющей молнии (цепное исцеление).
/// Исцеляет цель на healAmount, затем проверяет ближайшего союзника в радиусе 2 клеток
/// и если healAmount / 2 >= 1 — создаёт следующее событие цепи с уменьшенным лечением.
/// </summary>
public class HealingLightningEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    public int HealAmount { get; }

    /// <summary>Список уже задействованных в цепи целей (чтобы не повторяться).</summary>
    public List<Geid> AlreadyHealed { get; }

    public HealingLightningEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int healAmount, List<Geid> alreadyHealed = null)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        HealAmount = healAmount;
        AlreadyHealed = alreadyHealed ?? new List<Geid>();
        AlreadyHealed.Add(targetId);

        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
