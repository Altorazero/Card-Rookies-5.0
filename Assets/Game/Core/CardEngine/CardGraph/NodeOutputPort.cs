public sealed class NodeOutputPort
{
    public CardNode Owner { get; }

    public string Name { get; }

    internal NodeOutputPort(CardNode owner, string name)
    {
        Owner = owner;
        Name = name;
    }
}