using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntity
{
    Geid Id { get; }

    // Получить компонент, если он есть
    T GetComponent<T>() where T : class;

    // Проверить наличие компонента
    bool HasComponent<T>() where T : class;

    // Только для чтения: все компоненты (для снимков состояния)
    IReadOnlyDictionary<Type, object> Components { get; }
}

public sealed class BaseEntity : IEntity
{
    public Geid Id { get; }
    private readonly Dictionary<Type, object> _components = new();

    public IReadOnlyDictionary<Type, object> Components => _components;

    public BaseEntity()
    {
        Id = Geid.New;
    }

    internal BaseEntity(Geid existingId)
    {
        Id = existingId;
    }

    public void AddComponent<T>(T component) where T : class
    {
        _components[typeof(T)] = component;
    }

    public T GetComponent<T>() where T : class
    {
        _components.TryGetValue(typeof(T), out var comp);
        return comp as T;
    }

    public bool HasComponent<T>() where T : class
    {
        return _components.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Создаёт глубокую копию сущности с теми же ID и клонированными компонентами.
    /// </summary>
    public BaseEntity Clone()
    {
        var clone = new BaseEntity(Id);
        foreach (var kvp in _components)
        {
            var method = kvp.Value.GetType().GetMethod(
                "MemberwiseClone",
                BindingFlags.Instance | BindingFlags.NonPublic);
            clone._components[kvp.Key] = method?.Invoke(kvp.Value, null) ?? kvp.Value;
        }
        return clone;
    }
}