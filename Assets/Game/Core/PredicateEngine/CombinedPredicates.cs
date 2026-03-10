using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// Фабричные методы для создания составных предикатов: And / Or.
/// </summary>
public static class CompositePredicates
{
    public static IPredicate And(params IPredicate[] predicates) => new AndPredicate(predicates);

    public static IPredicate And(IEnumerable<IPredicate> predicates) => new AndPredicate(predicates);

    public static IPredicate Or(params IPredicate[] predicates) => new OrPredicate(predicates);

    public static IPredicate Or(IEnumerable<IPredicate> predicates) => new OrPredicate(predicates);
}

/// <summary>
/// Составное условие: пересечение (AND). Выполняется, если все вложенные условия истинны.
/// Пустой набор условий считается истинным (нейтральный элемент AND).
/// </summary>
public sealed class AndPredicate : ICompositePredicate
{
    private readonly ReadOnlyCollection<IPredicate> _predicates;

    public AndPredicate(IEnumerable<IPredicate> predicates)
    {
        if (predicates == null) throw new ArgumentNullException(nameof(predicates));
        var list = predicates.Where(c => c != null).ToList();
        _predicates = new ReadOnlyCollection<IPredicate>(list);
    }

    public AndPredicate(params IPredicate[] predicates) : this((IEnumerable<IPredicate>)predicates) { }

    public IReadOnlyList<IPredicate> Predicates => _predicates;

    public CompositeOperator Operator => CompositeOperator.And;

    public bool Evaluate(EventContext eventContext)
    {
        // короткое замыкание — если какое-то условие ложно, результат false
        foreach (var cond in _predicates)
        {
            if (!cond.Evaluate(eventContext))
                return false;
        }
        return true;
    }
}

/// <summary>
/// Составное условие: объединение (OR). Выполняется, если хотя бы одно вложенное условие истинно.
/// Пустой набор условий считается ложным (нейтральный элемент OR).
/// </summary>
public sealed class OrPredicate : ICompositePredicate
{
    private readonly ReadOnlyCollection<IPredicate> _predicates;

    public OrPredicate(IEnumerable<IPredicate> predicates)
    {
        if (predicates == null) throw new ArgumentNullException(nameof(predicates));
        var list = predicates.Where(c => c != null).ToList();
        _predicates = new ReadOnlyCollection<IPredicate>(list);
    }

    public OrPredicate(params IPredicate[] predicates) : this((IEnumerable<IPredicate>)predicates) { }

    public IReadOnlyList<IPredicate> Predicates => _predicates;

    public CompositeOperator Operator => CompositeOperator.Or;

    public bool Evaluate(EventContext eventContext)
    {
        // короткое замыкание — если какое-то условие истинно, результат true
        foreach (var cond in _predicates)
        {
            if (cond.Evaluate(eventContext))
                return true;
        }
        return false;
    }
}