using System;
using System.Collections.Generic;

public class Bindings
{
    private readonly Dictionary<object, object> _bindings = new();


    public void Set<T>(BindingKey<T> key, T value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        _bindings[key] = value;
    }


    public T Get<T>(BindingKey<T> key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (!_bindings.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Binding for key {key} not found.");

        return (T)value;
    }


    public bool TryGet<T>(BindingKey<T> key, out T value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (_bindings.TryGetValue(key, out var rawValue))
        {
            value = (T)rawValue;
            return true;
        }

        value = default;
        return false;
    }


    public bool Contains<T>(BindingKey<T> key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        return _bindings.ContainsKey(key);
    }


    public void Remove<T>(BindingKey<T> key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        _bindings.Remove(key);
    }
}