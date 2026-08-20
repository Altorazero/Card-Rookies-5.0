public sealed class NodeConnection
{
    public NodeOutputPort Output { get; }

    public CardNode Target { get; }

    public NodeConnection(
        NodeOutputPort output,
        CardNode target)
    {
        Output = output;
        Target = target;
    }
}