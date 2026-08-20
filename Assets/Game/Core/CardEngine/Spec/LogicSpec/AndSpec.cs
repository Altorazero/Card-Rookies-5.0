public sealed class AndSpec : IValueSpec<bool>
{
    public IValueSpec<bool> Left { get; }

    public IValueSpec<bool> Right { get; }


    public AndSpec(
        IValueSpec<bool> left,
        IValueSpec<bool> right)
    {
        Left = left;
        Right = right;
    }


    public bool Resolve(ExecutionContext context)
    {
        return Left.Resolve(context)
            && Right.Resolve(context);
    }
}