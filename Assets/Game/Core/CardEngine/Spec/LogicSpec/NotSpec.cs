public sealed class NotSpec : IValueSpec<bool>
{
    public IValueSpec<bool> Value { get; }


    public NotSpec(
        IValueSpec<bool> value)
    {
        Value = value;
    }


    public bool Resolve(ExecutionContext context)
    {
        return !Value.Resolve(context);
    }
}