using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Событие рикошета меча.
/// Наносит 4 урона цели. После попадания проверяет, есть ли другой противник
/// в радиусе 3 клеток от текущей цели и достаточно ли у кастера 1 маны и 1 энергии.
/// Если да — тратит ресурсы и создаёт следующий рикошет.
/// </summary>
public class RicochetSwordEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    /// <summary>Кастер меча.</summary>
    public Geid CasterEntityId { get; }

    /// <summary>Сущности, которые уже были поражены рикошетом.</summary>
    public List<Geid> AlreadyHit { get; }

    public RicochetSwordEvent(Geid systemSourceId, Geid casterEntityId, Geid targetId, List<Geid> alreadyHit = null)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        CasterEntityId = casterEntityId;
        AlreadyHit = alreadyHit ?? new List<Geid>();
        AlreadyHit.Add(targetId);

        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, casterEntityId),
            (SubjectRole.Target, targetId)
        );
    }
}
