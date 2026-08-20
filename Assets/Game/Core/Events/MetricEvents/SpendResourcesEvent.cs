using System.Collections.Generic;

/// <summary>
/// Событие трат ресурсов (маны и/или энергии) сущности.
/// Guard-фаза проверяет наличие ресурсов; при недостатке отменяет событие.
/// Apply-фаза списывает ресурсы.
/// </summary>
public class SpendResourcesEvent : IGameEvent, IGuardPhaseEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }

    /// <summary>Стоимость маны.</summary>
    public int Amount { get; }

    /// <summary>Тип ресурса.</summary>
    public MetricResourceType ResourceType { get; }
    public EventScratch Scratch { get; set; }

    public System.Collections.Generic.Dictionary<Role, System.Collections.Generic.List<IEntity>> Subjects { get; set; }

    /// <summary>Идентификатор сущности, которая тратит ресурсы.</summary>
    public IEntity SpenderEntityId { get; }

    public SpendResourcesEvent(GEID systemSourceId, IEntity source, IEntity spenderEntityId, int amount, MetricResourceType resourceType)
    {
        Id = GEID.New;
        SystemSourceId = systemSourceId;
        SpenderEntityId = spenderEntityId;
        Amount = amount;
        ResourceType = resourceType;
        Subjects = SubjectsHelper.Create((Role.Source, source));
        Subjects.Add(Role.Target, new List<IEntity>() { spenderEntityId });
    }
}
