using System;
using System.Collections.Generic;
using System.Reflection;

public interface IEntity
{
    GEID Id { get; }

    // Получить компонент, если он есть
    T GetComponent<T>() where T : class;

    // Проверить наличие компонента
    bool HasComponent<T>() where T : class;

    // Только для чтения: все компоненты (для снимков состояния)
    IReadOnlyDictionary<Type, object> Components { get; }

    void AddComponent<T>(T component) where T : class;
    void RemoveComponent<T>() where T : class;

    public IEntity Empty();
}

// ===== 2. BaseEntity — упрощаем Clone(), убираем reflection =====
public class BaseEntity : IEntity
{
    public GEID Id { get; }
    private readonly Dictionary<Type, object> _components = new();
    public IReadOnlyDictionary<Type, object> Components => _components;

    public BaseEntity() => Id = GEID.New;
    internal BaseEntity(GEID existingId) => Id = existingId;

    public void AddComponent<T>(T component) where T : class => _components[typeof(T)] = component;
    public T GetComponent<T>() where T : class => _components.TryGetValue(typeof(T), out var c) ? c as T : null;
    public bool HasComponent<T>() where T : class => _components.ContainsKey(typeof(T));
    public void RemoveComponent<T>() where T : class => _components.Remove(typeof(T));

    // Больше никакой рефлексии и MemberwiseClone.
    // Shallow copy корректен, потому что компоненты неизменяемы.
    public BaseEntity Clone()
    {
        var clone = new BaseEntity(Id);
        foreach (var kvp in _components)
            clone._components[kvp.Key] = kvp.Value;
        return clone;
    }

    public BaseEntity Empty() => new();
    IEntity IEntity.Empty() => Empty();
}


