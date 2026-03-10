/// <summary>
/// Шаг-пул: очищает список кандидатов (явно пустое начальное состояние).
/// Полезен при построении нескольких пулов в одном пайплайне:
/// первый пул не нуждается в EmptyPool, но второй цикл (после CommitAndStop) может начинаться с очистки.
/// </summary>
public class EmptyPool : ITargetingStep
{
    public void Execute(TargetingContext context)
    {
        context.Candidates.Clear();
    }
}
