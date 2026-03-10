using UnityEngine;

/// <summary>
/// Система логирования: отображает начало каждой фазы обработки события.
/// Должна быть зарегистрирована с наименьшим приоритетом (вызывается первой в фазе).
/// </summary>
public class PhaseStartLogSystem : IEventListener<IGameEvent, IPhaseEvent>
{
    public int Priority { get; } = int.MinValue;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<IGameEvent, IPhaseEvent>.OnEvent(EventContext context, IGameEvent evt)
    {
        var phaseName = context.CurrentPhase?.Name ?? "Unknown";
        Debug.Log($"[PHASE START] {phaseName} | Event: {evt.GetType().Name} | Status: {evt.Status}");
    }
}

/// <summary>
/// Система логирования: отображает завершение каждой фазы обработки события.
/// Должна быть зарегистрирована с наибольшим приоритетом (вызывается последней в фазе).
/// </summary>
public class PhaseEndLogSystem : IEventListener<IGameEvent, IPhaseEvent>
{
    public int Priority { get; } = int.MaxValue;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<IGameEvent, IPhaseEvent>.OnEvent(EventContext context, IGameEvent evt)
    {
        var phaseName = context.CurrentPhase?.Name ?? "Unknown";
        Debug.Log($"[PHASE END]   {phaseName} | Event: {evt.GetType().Name} | Status: {evt.Status}");
    }
}
