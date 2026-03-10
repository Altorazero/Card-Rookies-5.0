/// <summary>
/// Шаг пайплайна: проверяет условие на текущих кандидатах и выполняет соответствующее действие.
///
/// Пример — «нужна хотя бы одна цель, иначе fizzle»:
/// <code>
/// new ExitConditionStep(
///     new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
///     onNotMet: new FizzleTargetingAction()
/// )
/// </code>
///
/// Пример — «при перегрузке целей (> 5) — альтернативный эффект»:
/// <code>
/// new ExitConditionStep(
///     new CandidateCountPredicate(ComparisonOperator.GreaterThan, 5),
///     onMet: new AlternativeEffectAction(ctx => new SplashEvent(...))
/// )
/// </code>
/// </summary>
public class ExitConditionStep : ITargetingStep
{
    /// <summary>Условие, проверяемое на текущих кандидатах.</summary>
    public ITargetListPredicate Predicate { get; }

    /// <summary>Действие при выполнении условия. null — ничего не делать, продолжить.</summary>
    public ITargetingAction OnMet { get; }

    /// <summary>Действие при невыполнении условия. null — ничего не делать, продолжить.</summary>
    public ITargetingAction OnNotMet { get; }

    public ExitConditionStep(
        ITargetListPredicate predicate,
        ITargetingAction onMet = null,
        ITargetingAction onNotMet = null)
    {
        Predicate = predicate;
        OnMet = onMet;
        OnNotMet = onNotMet;
    }

    public void Execute(TargetingContext context)
    {
        bool met = Predicate.Evaluate(context.Candidates, context.EventContext);
        if (met)
            OnMet?.Execute(context);
        else
            OnNotMet?.Execute(context);
    }
}
