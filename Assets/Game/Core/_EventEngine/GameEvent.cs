using System.Collections.Generic;

public enum EventStatus
{
    Pending,     // В очереди, ещё не обрабатывалось
    WaitingForInput, // Ожидает выбора игрока (приостанавливает очередь)
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
    GEID Id { get; }

    /// <summary>
    /// Id системы, породившей данный Event. Нужно для отладки, чтобы понять откуда!
    /// </summary>
    GEID SystemSourceId { get; }
    EventScratch Scratch { get; }

}

public class GameEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public GEID Id { get; }
    public GEID SystemSourceId { get; }

    public EventScratch Scratch { get; }

    public GameEvent(GEID systemSourceId)
    {
        Id = GEID.New;
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
/// Роли субъектов в событии с Subjects.
/// Source = 0, Target = 1, Owner = 2, Auxiliary = 3, PrimaryTarget = 4, SecondaryTarget = 5
/// </summary>
public enum Role
{
    Source = 0,
    Target = 1,
    Owner = 2,
    Auxiliary = 3,
    PrimaryTarget = 4,
    SecondaryTarget = 5,
    Spender = 6,
}

/// <summary>
/// Интерфейс для событий с участием сущностей.
/// Subjects — список списков GEID, где индекс соответствует значению Role:
///   Subjects[(int)Role.Source]  — источники
///   Subjects[(int)Role.Target]  — цели
///   и т.д.
/// </summary>
public interface IHaveSubjects : IGameEvent
{
    System.Collections.Generic.Dictionary<Role, System.Collections.Generic.List<IEntity>> Subjects { get; set; }
}

// Тот же принцип, что Bindings, но с собственным пространством имён —
// подчёркивает, что это данные ЭТОГО события, а не карты.
public sealed class EventScratch
{
    private readonly Dictionary<object, object> _data = new();

    public void Set<T>(BindingKey<T> key, T value) => _data[key] = value;

    public bool TryGet<T>(BindingKey<T> key, out T value)
    {
        if (_data.TryGetValue(key, out var raw))
        {
            value = (T)raw;
            return true;
        }
        value = default;
        return false;
    }

    public T GetOrDefault<T>(BindingKey<T> key) =>
        TryGet(key, out var value) ? value : default;
}

public static class BuiltinScratchKeys
{
    public static readonly BindingKey<int> ShieldAbsorbed = new("ShieldAbsorbed");
    public static readonly BindingKey<int> ShieldValueBeforeApply = new("ShieldValueBeforeApply");
}