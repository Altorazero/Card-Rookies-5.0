using System;
using System.Linq;
using UnityEditor.Search;

public class EventContext
{
    public BattleState BattleState { get; }
    public IGameEvent Event { get; }
    public EventQueue Dispatcher { get; }
    public CommandLog CommandLog { get; }
    public ExecutionMode Mode { get; }
    public bool IsReal => Mode == ExecutionMode.Real;

    // Внедряем сервис интерактивности (DI)
    public IInteractionService Interaction { get; }
    public Type CurrentPhase { get; internal set; }
    public EventContext(BattleState state, IGameEvent evt, EventQueue dispatcher, IInteractionService interaction = null)
    {
        BattleState = state;
        Event = evt;
        Dispatcher = dispatcher;
        Interaction = interaction;
        CommandLog = dispatcher.CommandLog;
        Mode = dispatcher.Mode;
    }

    private static readonly Type[] AllowedRaisePhases =
   {
        typeof(IAfterPhaseEvent),
        typeof(ISBAEvent),
    };

    public void Raise(IGameEvent newEvent, bool atFront = false)
    {
        if (!AllowedRaisePhases.Any(p => p.IsAssignableFrom(CurrentPhase)))
            throw new InvalidOperationException(
                $"Raise is only allowed in After/SBA phases (was {CurrentPhase.Name}). " +
                $"Use Replace() during the Replace phase to substitute the current event.");

        Dispatcher.Enqueue(newEvent, atFront);
    }

    /// <summary>
    /// Подменяет текущее событие другим. Разрешено только на фазе Replace.
    /// Исходное событие немедленно останавливается (не проходит Modify/TargetResolve/Apply/After/SBA),
    /// заменяющее событие встаёт в начало очереди и пройдёт полный цикл фаз с нуля.
    /// </summary>
    public void Replace(IGameEvent replacement)
    {
        if (!typeof(IReplacePhaseEvent).IsAssignableFrom(CurrentPhase))
            throw new InvalidOperationException(
                $"Replace is only allowed in the Replace phase (was {CurrentPhase.Name}).");

        if (Event.Status == EventStatus.Replaced)
            throw new InvalidOperationException(
                $"Event {Event} was already replaced this phase. " +
                $"Only one Replace system should win — use Priority to order competing replacement systems explicitly.");

        Event.Status = EventStatus.Replaced;
        Dispatcher.Enqueue(replacement, atFront: true);
    }
}
