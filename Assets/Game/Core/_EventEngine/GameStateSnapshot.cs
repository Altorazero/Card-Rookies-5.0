using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Снимок состояния BattleState в определённый момент времени.
/// Хранит копии компонентов всех сущностей на момент снятия снимка.
/// </summary>
public sealed class GameStateSnapshot
{
    /// <summary>
    /// Временная метка создания снимка (UTC).
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Снимок данных сущностей: entityId → (componentType → cloned component value).
    /// </summary>
    public IReadOnlyDictionary<Geid, IReadOnlyDictionary<Type, object>> EntitySnapshots { get; }

    public GameStateSnapshot(BattleState state)
    {
        Timestamp = DateTime.UtcNow;
        EntitySnapshots = TakeSnapshot(state);
    }

    private static IReadOnlyDictionary<Geid, IReadOnlyDictionary<Type, object>> TakeSnapshot(BattleState state)
    {
        var snapshot = new Dictionary<Geid, IReadOnlyDictionary<Type, object>>();
        foreach (var kvp in state.Entities)
        {
            snapshot[kvp.Key] = TakeEntitySnapshot(kvp.Value);
        }
        return snapshot;
    }

    private static IReadOnlyDictionary<Type, object> TakeEntitySnapshot(IEntity entity)
    {
        var compSnapshot = new Dictionary<Type, object>();
        foreach (var kvp in entity.Components)
        {
            // MemberwiseClone через рефлексию.
            // ВАЖНО: работает корректно только если все поля компонента являются value-типами
            // или иммутабельными объектами. Если компонент содержит изменяемые reference-типы,
            // снимок будет неполным (shallow copy).
            var method = kvp.Value.GetType().GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var clone = method?.Invoke(kvp.Value, null) ?? kvp.Value;
            compSnapshot[kvp.Key] = clone;
        }
        return compSnapshot;
    }
}
