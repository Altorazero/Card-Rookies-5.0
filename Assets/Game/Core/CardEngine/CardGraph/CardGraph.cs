
using System.Collections.Generic;
using System.Linq;
public sealed class CardGraph
{
    public CardNode EntryNode { get; }

    public IReadOnlyList<CardNode> Nodes => _nodes;

    public IReadOnlyList<NodeConnection> Connections => _connections;

    private readonly List<CardNode> _nodes = new();

    private readonly List<NodeConnection> _connections = new();

    public CardGraph(CardNode entry)
    {
        EntryNode = entry;
        AddNode(entry);
    }

    public void AddNode(CardNode node)
    {
        _nodes.Add(node);
    }

    public void Connect(
        NodeOutputPort output,
        CardNode target)
    {
        _connections.Add(
            new NodeConnection(output, target));
    }
#nullable enable
    public CardNode? GetNext(NodeOutputPort port)
    {
        return _connections
            .FirstOrDefault(c => c.Output == port)
            ?.Target;
    }
}
