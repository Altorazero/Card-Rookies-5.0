using System.Collections.Generic;

/// <summary>
/// Предикат, оценивающий текущий список кандидатов в пайплайне таргетинга.
/// Используется в <see cref="ExitConditionStep"/> для принятия решения о продолжении пайплайна.
/// </summary>
public interface ITargetListPredicate
{
    bool Evaluate(IReadOnlyList<Geid> candidates, EventContext context);
}
