public sealed class BindingSpec<T> : IValueSpec<T>
{
    public BindingKey<T> Variable { get; }

    public BindingSpec(BindingKey<T> variable)
    {
        Variable = variable;
    }

    public T Resolve(ExecutionContext context)
    {
        return context.Bindings.Get(Variable);
    }
}