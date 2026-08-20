using System.Collections.Generic;

/// <summary>
/// Шаг 2: Фильтрует или трансформирует пул кандидатов.
/// </summary>
public interface ICandidateTransform<T>
{
    IEnumerable<T> Transform(IEnumerable<T> candidates, ExecutionContext context);
}
