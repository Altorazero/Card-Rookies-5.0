/// <summary>
/// Одна ветвь события ветвления: условие и порождаемое событие.
/// </summary>
public class BranchEntry
{
    /// <summary>Условие активации ветви. Null означает безусловное выполнение.</summary>
    public IPredicate Condition { get; }

    /// <summary>Событие, порождаемое при выполнении условия.</summary>
    public IGameEvent Effect { get; }

    public BranchEntry(IPredicate condition, IGameEvent effect)
    {
        Condition = condition;
        Effect = effect;
    }
}
