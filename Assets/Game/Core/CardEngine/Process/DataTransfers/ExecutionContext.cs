public sealed class ExecutionContext
{
    public EventContext EventContext { get; }
    public Bindings Bindings { get; }
    public CardInstance Card { get; }
    public ISelectionOverrideSource SelectionOverrides { get; init; }

    public ExecutionContext(
    EventContext eventContext,
    CardInstance card)
    {
        EventContext = eventContext;
        Card = card;
        Bindings = new Bindings();
    }
}
