using System.Collections.Generic;

public interface IPredicate
{
    bool Evaluate(EventContext eventContext);

}


/// <summary>
/// Оператор объединения для составного условия.
/// </summary>
public enum CompositeOperator
{
    /// <summary>Условие выполняется, если все вложенные условия истинны (пересечение / AND).</summary>
    And,

    /// <summary>Условие выполняется, если хотя бы одно из вложенных условий истинно (объединение / OR).</summary>
    Or
}

/// <summary>
/// Интерфейс для комбинированного условия — составного условия, которое агрегирует несколько
/// существующих <see cref="IPredicate"/> и оценивается как объединение или пересечение этих условий.
/// Наследуется от <see cref="IPredicate"/> чтобы быть взаимозаменяемым с обычными условиями.
/// </summary>
public interface ICompositePredicate : IPredicate
{
    /// <summary>
    /// Вложенные условия, которые будут проверяться при вычислении составного условия.
    /// </summary>
    IReadOnlyList<IPredicate> Predicates { get; }

    /// <summary>
    /// Оператор объединения (And / Or), задающий семантику композиции.
    /// </summary>
    CompositeOperator Operator { get; }
}


