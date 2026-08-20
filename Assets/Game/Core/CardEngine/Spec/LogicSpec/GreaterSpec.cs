public sealed class GreaterSpec : IValueSpec<bool>
{
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public GreaterSpec(
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Left = left;
        Right = right;
    }

    public bool Resolve(ExecutionContext context)
    {
        return Left.Resolve(context) >
               Right.Resolve(context);
    }
}