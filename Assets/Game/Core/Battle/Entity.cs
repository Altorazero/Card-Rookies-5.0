using System;
using System.Collections.Generic;

public interface IEntity
{
    Geid Id { get; }

    // Получение компонента, если он есть
    T GetComponent<T>() where T : class;

    // Проверка наличия компонента
    bool HasComponent<T>() where T : class;
}

public sealed class BaseEntity : IEntity
{
    public Geid Id { get; }
    private readonly Dictionary<Type, object> _components = new();

    public BaseEntity()
    {
        Id = Geid.New;
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
}