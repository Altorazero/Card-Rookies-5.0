public class EventContext
{
    public BattleState BattleState { get; }
    public IGameEvent Event { get; }
    public EventDispatcher Dispatcher { get; }
    
    /// <summary>
    /// Текущий кандидат при оценке таргетинга. Null вне контекста таргетинга.
    /// </summary>
    public Geid? EvaluatingCandidate { get; internal set; }

    public EventContext(BattleState battleState, IGameEvent @event, EventDispatcher dispatcher)
    {
        BattleState = battleState;
        Event = @event;
        Dispatcher = dispatcher;
    }
}