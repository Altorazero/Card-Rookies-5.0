public sealed class CurrentTargetSpec : IValueSpec<IEntity>
{
    public IEntity Resolve(ExecutionContext context)
    {
        return context.Bindings.Get(BuiltInBindings.CurrentTarget);
    }
}