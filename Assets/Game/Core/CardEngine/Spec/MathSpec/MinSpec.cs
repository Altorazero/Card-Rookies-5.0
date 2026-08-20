using System;

public sealed class MinSpec : IValueSpec<int>
{
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public MinSpec(
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Left = left;
        Right = right;
    }

    public int Resolve(ExecutionContext context)
    {
        return Math.Min(Left.Resolve(context),
               Right.Resolve(context));
    }
}