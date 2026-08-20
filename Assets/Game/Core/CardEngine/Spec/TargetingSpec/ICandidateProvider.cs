using System.Collections.Generic;

/// <summary>
/// Шаг 1: Предоставляет начальный пул кандидатов для таргетинга.
/// </summary>
public interface ICandidateProvider<T>
{
    IEnumerable<T> GetValues(ExecutionContext context);
}
