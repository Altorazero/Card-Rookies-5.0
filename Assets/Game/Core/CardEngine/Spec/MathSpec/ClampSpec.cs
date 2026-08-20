using System;

public sealed class ClampSpec : IValueSpec<int>
{
    public IValueSpec<int> Value { get; }
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public ClampSpec(
        IValueSpec<int> value,
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Value = value;
        Left = left;
        Right = right;
    }

    public int Resolve(ExecutionContext context)
    {
        return Math.Clamp(Value.Resolve(context),Left.Resolve(context),
               Right.Resolve(context));
    }
}