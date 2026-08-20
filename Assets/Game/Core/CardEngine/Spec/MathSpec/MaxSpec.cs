using System;

public sealed class MaxSpec : IValueSpec<int>
{
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public MaxSpec(
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Left = left;
        Right = right;
    }

    public int Resolve(ExecutionContext context)
    {
        return Math.Max(Left.Resolve(context),
               Right.Resolve(context));
    }
}