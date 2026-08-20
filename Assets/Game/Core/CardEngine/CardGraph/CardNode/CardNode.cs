using System.Collections.Generic;


public abstract class CardNode
{
    public GEID Id { get; } = GEID.New;

    public IReadOnlyList<NodeOutputPort> Outputs => _outputs;

    private readonly List<NodeOutputPort> _outputs = new();

    protected NodeOutputPort AddOutput(string name)
    {
        var port = new NodeOutputPort(this, name);
        _outputs.Add(port);
        return port;
    }
}


public enum NodeOutcomeKind { Advance, Suspend, Cancel }

public readonly struct NodeOutcome
{
    public NodeOutcomeKind Kind { get; }
    public CardNode Next { get; }
    public ISuspendPoint SuspendPoint { get; }

    public static NodeOutcome Advance(CardNode next) => new(NodeOutcomeKind.Advance, next, null);
    public static NodeOutcome Suspend(ISuspendPoint point) => new(NodeOutcomeKind.Suspend, null, point);
    public static NodeOutcome Cancel() => new(NodeOutcomeKind.Cancel, null, null);

    private NodeOutcome(NodeOutcomeKind kind, CardNode next, ISuspendPoint point)
    {
        Kind = kind;
        Next = next;
        SuspendPoint = point;
    }
}

