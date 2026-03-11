using System.Collections.Generic;

/// <summary>
/// Событие трат ресурсов (маны и/или энергии) сущности.
/// Guard-фаза проверяет наличие ресурсов; при недостатке отменяет событие.
/// Apply-фаза списывает ресурсы.
/// </summary>
public class SpendResourcesEvent : IGameEvent, IGuardPhaseEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Стоимость маны.</summary>
    public int ManaCost { get; }

    /// <summary>Стоимость энергии.</summary>
    public int EnergyCost { get; }

    public List<List<Geid>> Subjects { get; set; }

    /// <summary>Идентификатор сущности, которая тратит ресурсы.</summary>
    public Geid SpenderEntityId { get; }

    public SpendResourcesEvent(Geid systemSourceId, Geid spenderEntityId, int manaCost, int energyCost)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        SpenderEntityId = spenderEntityId;
        ManaCost = manaCost;
        EnergyCost = energyCost;
        Subjects = SubjectsHelper.Create((SubjectRole.Source, spenderEntityId));
    }
}
