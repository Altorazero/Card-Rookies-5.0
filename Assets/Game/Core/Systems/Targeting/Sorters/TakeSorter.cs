/// <summary>
/// Шаг пайплайна: оставляет только первые <see cref="Count"/> кандидатов.
/// Применяется после сортировщиков для ограничения числа целей.
/// </summary>
public class TakeSorter : ITargetingStep
{
    public int Count { get; }

    public TakeSorter(int count)
    {
        Count = count;
    }

    public void Execute(TargetingContext context)
    {
        if (context.Candidates.Count > Count)
            context.Candidates.RemoveRange(Count, context.Candidates.Count - Count);
    }
}
