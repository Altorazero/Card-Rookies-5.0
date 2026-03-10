using System.Collections.Generic;

public enum EventStatus
{
    Pending,     // В очереди, ещё не обрабатывалось
    Cancelled,   // Отменено до применения
    Replaced,    // Заменено другим событием
    Fizzled,     // Не нашло цели и т.п.
    Applied      // Успешно применено
}

public interface IGameEvent
{
    /// <summary>
    /// Статус события.
    /// Pending - в очереди, ещё не обрабатывалось
    /// </summary>
    EventStatus Status { get; set; }

    /// <summary>
    /// Уникальный ID Event.
    /// </summary>
    Geid Id { get; }

    /// <summary>
    /// Id системы, породившей данный Event. Нужно для отладки, чтобы понять откуда!
    /// </summary>
    Geid SystemSourceId { get; }
}

public class GameEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public GameEvent(Geid systemSourceId)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
    }
}

// Интерфейс для помещения фаз событий
public interface IPhaseEvent : IGameEvent { }
// Фазы обработки события
public interface IGuardPhaseEvent : IPhaseEvent { }
public interface IReplacePhaseEvent : IPhaseEvent { }
public interface IModifyPhaseEvent : IPhaseEvent { }
public interface ITargetResolvePhaseEvent : IPhaseEvent { }
public interface IApplyPhaseEvent : IPhaseEvent { }
public interface IAfterPhaseEvent : IPhaseEvent { }
public interface ISBAEvent : IPhaseEvent { }

/// <summary>
/// Роли субъектов в событии. Порядок enum-значений определяет индексы в Subjects.
/// Source = 0, Target = 1, Owner = 2, Auxiliary = 3, PrimaryTarget = 4, SecondaryTarget = 5
/// </summary>
public enum SubjectRole
{
    Source = 0,
    Target = 1,
    Owner = 2,
    Auxiliary = 3,
    PrimaryTarget = 4,
    SecondaryTarget = 5,
}

/// <summary>
/// Интерфейс для событий с участием сущностей.
/// Subjects — список списков Geid, где индекс соответствует значению SubjectRole:
///   Subjects[(int)SubjectRole.Source]  — источники
///   Subjects[(int)SubjectRole.Target]  — цели
///   и т.д.
/// </summary>
public interface IHaveSubjects : IGameEvent
{
    List<List<Geid>> Subjects { get; set; }
}