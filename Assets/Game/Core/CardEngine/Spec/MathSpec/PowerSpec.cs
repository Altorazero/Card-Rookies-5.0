using System;

public sealed class PowerSpec : IValueSpec<int>
{
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public PowerSpec(
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Left = left;
        Right = right;
    }

    public int Resolve(ExecutionContext context)
    {
        return (int)Math.Pow(Left.Resolve(context),
               Right.Resolve(context));
    }
}