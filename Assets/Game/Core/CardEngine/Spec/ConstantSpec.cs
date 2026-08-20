public sealed class ConstantSpec<T> : IValueSpec<T>
{
    public T Value { get; }

    public ConstantSpec(T value)
    {
        Value = value;
    }

    public T Resolve(ExecutionContext context)
    {
        return Value;
    }
}