using System;

public class EventContext
{
    public BattleState BattleState { get; }
    public IGameEvent Event { get; }
    public EventQueue Dispatcher { get; }

    /// <summary>
    /// Текущая фаза, обрабатываемая EventQueue. Null вне обработки фаз.
    /// </summary>
    public Type CurrentPhase { get; internal set; }

    /// <summary>
    /// Текущий кандидат при таргетинге. Null при обычной обработке.
    /// </summary>
    public Geid? EvaluatingCandidate { get; internal set; }

    public EventContext(BattleState battleState, IGameEvent @event, EventQueue dispatcher)
    {
        BattleState = battleState;
        Event = @event;
        Dispatcher = dispatcher;
    }
}