public sealed class CasterSpec : IValueSpec<IEntity>
{
    public IEntity Resolve(ExecutionContext context)
    {
        return context.Bindings.Get(BuiltInBindings.Caster);
    }
}