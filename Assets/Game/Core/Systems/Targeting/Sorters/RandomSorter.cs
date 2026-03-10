/// <summary>
/// Шаг пайплайна: перемешивает кандидатов в случайном порядке (Fisher–Yates shuffle).
/// Использует <see cref="BattleRng"/> из BattleState для детерминированности при фиксированном seed.
/// </summary>
public class RandomSorter : ITargetingStep
{
    public void Execute(TargetingContext context)
    {
        var rng = context.EventContext.BattleState.Rng;
        var list = context.Candidates;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
