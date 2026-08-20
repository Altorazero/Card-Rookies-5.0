using System;

public static class BattleStateMutationExtensions
{
    public static void Mutate<T>(this BattleState state, CommandLog log, GEID entityId, Func<T, T> modifier)
        where T : class, IComponent
    {
        var entity = state.GetEntity(entityId);
        var oldValue = entity.GetComponent<T>();
        var newValue = modifier(oldValue);
        log.Apply(new SetComponentCommand<T>(entityId, oldValue, newValue));
    }
}

public static class ExecutionContextExtensions
{
    public static void Mutate<T>(this ExecutionContext ctx, GEID entityId, Func<T, T> modifier)
        where T : class, IComponent
        => ctx.EventContext.BattleState.Mutate(ctx.EventContext.CommandLog, entityId, modifier);

    public static void Mutate<T>(this EventContext ctx, GEID entityId, Func<T, T> modifier)
    where T : class, IComponent
    => ctx.BattleState.Mutate(ctx.CommandLog, entityId, modifier);
}