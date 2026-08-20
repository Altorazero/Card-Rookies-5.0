public sealed class SubtractSpec : IValueSpec<int>
{
    public IValueSpec<int> Left { get; }

    public IValueSpec<int> Right { get; }

    public SubtractSpec(
        IValueSpec<int> left,
        IValueSpec<int> right)
    {
        Left = left;
        Right = right;
    }

    public int Resolve(ExecutionContext context)
    {
        return Left.Resolve(context) -
               Right.Resolve(context);
    }
}