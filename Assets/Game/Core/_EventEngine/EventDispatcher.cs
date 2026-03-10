using System;
using System.Collections.Generic;
using System.Linq;

// Generic listener signature
public interface IBaseEventListener
{
    Geid SystemId { get; }
    int Priority { get; }
}

public interface IEventListener<TEvent, TPhase> : IBaseEventListener
    where TEvent : IGameEvent
    where TPhase : IPhaseEvent
{
    void OnEvent(EventContext context, TEvent evt);
}

// Специальный слушатель для SBA-фазы
public interface ISBAListener : IBaseEventListener
{
    void OnSBA(EventContext context);
}

/// <summary>
/// Очередь событий с поддержкой:
/// - фазовой обработки (Guard → Replace → Modify → TargetResolve → Apply → After → SBA)
/// - барьерной очереди с предикатами-блоками
/// - снимков состояния GameState перед обработкой внешних событий
/// </summary>
public class EventQueue
{
    // Основная очередь: (событие, нужен ли снимок перед обработкой)
    private readonly LinkedList<(IGameEvent Event, bool NeedsSnapshot)> _mainQueue = new();

    // Барьерная очередь: (событие, предикат-блок, было ли добавлено извне)
    private readonly List<(IGameEvent Event, IPredicate Predicate, bool IsExternal)> _barrierQueue = new();

    // Зарегистрированные слушатели
    private readonly List<(
        object listener,
        Type eventType,
        Type phaseType,
        Action<EventContext> invoker,
        int priority
    )> _listeners = new();

    public BattleState BattleState;

    /// <summary>
    /// История снимков состояния (создаётся перед каждым внешним событием).
    /// </summary>
    public List<GameStateSnapshot> StateHistory { get; } = new();

    private bool _isProcessing;

    private static readonly Type[] PhaseOrder =
    {
        typeof(IGuardPhaseEvent),
        typeof(IReplacePhaseEvent),
        typeof(IModifyPhaseEvent),
        typeof(ITargetResolvePhaseEvent),
        typeof(IApplyPhaseEvent),
        typeof(IAfterPhaseEvent),
        typeof(ISBAEvent),
    };

    public EventQueue(BattleState state)
    {
        BattleState = state;
    }

    // Универсальная подписка системы — регистрирует все реализованные IEventListener<,>
    public void Subscribe(IBaseEventListener system)
    {
        if (system == null) return;

        var systemType = system.GetType();
        var interfaces = systemType.GetInterfaces();

        if (system is ISBAListener sbaListener)
            SubscribeSBAListener(sbaListener);

        foreach (var iface in interfaces)
        {
            if (!iface.IsGenericType) continue;

            var genericDef = iface.GetGenericTypeDefinition();
            if (genericDef != typeof(IEventListener<,>)) continue;

            var typeArgs = iface.GetGenericArguments();
            var eventType = typeArgs[0];
            var phaseType = typeArgs[1];

            if (_listeners.Any(e => ReferenceEquals(e.listener, system) &&
                                    e.eventType == eventType &&
                                    e.phaseType == phaseType))
                continue;

            var method = iface.GetMethod("OnEvent");
            Action<EventContext> invoker = ctx =>
            {
                if (eventType.IsInstanceOfType(ctx.Event))
                    method.Invoke(system, new object[] { ctx, ctx.Event });
            };

            _listeners.Add((system, eventType, phaseType, invoker, system.Priority));
        }

        _listeners.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    // Явная подписка generic-слушателя
    public void Subscribe<TEvent, TPhase>(IEventListener<TEvent, TPhase> listener)
        where TEvent : IGameEvent
        where TPhase : IPhaseEvent
    {
        if (listener == null) return;
        if (_listeners.Any(e => ReferenceEquals(e.listener, listener))) return;

        Action<EventContext> invoker = ctx =>
        {
            if (ctx.Event is TEvent evt)
                listener.OnEvent(ctx, evt);
        };

        _listeners.Add((listener, typeof(TEvent), typeof(TPhase), invoker, listener.Priority));
        _listeners.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    private void SubscribeSBAListener(ISBAListener listener)
    {
        if (_listeners.Any(e => ReferenceEquals(e.listener, listener) && e.phaseType == typeof(ISBAEvent)))
            return;

        Action<EventContext> invoker = ctx => listener.OnSBA(ctx);

        _listeners.Add((listener, typeof(IGameEvent), typeof(ISBAEvent), invoker, listener.Priority));
    }

    /// <summary>
    /// Добавить событие в очередь. Если вызывается вне обработки — помечается как внешнее
    /// (перед его обработкой будет сохранён снимок состояния).
    /// </summary>
    public void Enqueue(IGameEvent action, bool atFront = false)
    {
        bool isExternal = !_isProcessing;
        if (atFront)
            _mainQueue.AddFirst((action, isExternal));
        else
            _mainQueue.AddLast((action, isExternal));
    }

    /// <summary>
    /// Добавить событие в барьерную очередь с необязательным предикатом-блоком.
    /// Событие будет перемещено в основную очередь только когда предикат станет истинным
    /// (проверка происходит при опустении основной очереди).
    /// </summary>
    public void EnqueueWithBarrier(IGameEvent action, IPredicate predicate = null)
    {
        bool isExternal = !_isProcessing;
        _barrierQueue.Add((action, predicate, isExternal));
    }

    /// <summary>
    /// Обработать все события в основной очереди.
    /// </summary>
    public void ProcessQueue()
    {
        if (_isProcessing) return;

        _isProcessing = true;

        while (_mainQueue.Count > 0)
        {
            var (action, needsSnapshot) = _mainQueue.First.Value;
            _mainQueue.RemoveFirst();

            if (needsSnapshot)
                StateHistory.Add(new GameStateSnapshot(BattleState));

            foreach (var phase in PhaseOrder)
            {
                if (action.Status == EventStatus.Cancelled)
                    break;

                var context = new EventContext(BattleState, action, this);
                context.CurrentPhase = phase;

                foreach (var entry in _listeners)
                {
                    if (action.Status == EventStatus.Cancelled)
                        break;

                    if (!entry.phaseType.IsAssignableFrom(phase))
                        continue;

                    if (!entry.eventType.IsInstanceOfType(action))
                        continue;

                    entry.invoker(context);
                }
            }
        }

        _isProcessing = false;
        ProcessBarrierQueue();
    }

    private void ProcessBarrierQueue()
    {
        bool anyReleased = false;

        for (int i = _barrierQueue.Count - 1; i >= 0; i--)
        {
            var (action, predicate, isExternal) = _barrierQueue[i];
            if (CheckBarrierResolved(action, predicate))
            {
                _barrierQueue.RemoveAt(i);
                // Сохраняем флаг isExternal при перемещении в основную очередь
                _mainQueue.AddLast((action, isExternal));
                anyReleased = true;
            }
        }

        if (anyReleased && _mainQueue.Count > 0)
            ProcessQueue();
    }

    private bool CheckBarrierResolved(IGameEvent gameEvent, IPredicate predicate)
    {
        if (predicate == null) return true;
        var context = new EventContext(BattleState, gameEvent, this);
        return predicate.Evaluate(context);
    }
}
