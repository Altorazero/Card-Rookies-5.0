using System;
using System.Collections.Generic;
using System.Linq;

// Generic listener signature
public interface IBaseEventListener
{
    GEID SystemId { get; }
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
public enum ExecutionMode
{
    Real,        // настоящий розыгрыш, мутации остаются, логи/эффекты как обычно
    Preview,     // "что будет, если..." — резолвится по-настоящему, но обязан откатиться
    Simulation,  // AI-перебор — как Preview, но без UI-побочки (без логов, без обращений к Interaction)
}
public interface IExecutionScope : IDisposable
{
    /// <summary>Оставить результат Preview/Simulation в состоянии — редкий случай, обычно не нужен.</summary>
    void Commit();
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
    private readonly List<(IGameEvent Event, IValueSpec<bool> Predicate, bool IsExternal)> _barrierQueue = new();

    // Зарегистрированные слушатели
    private readonly List<(
        object listener,
        Type eventType,
        Type phaseType,
        Action<EventContext> invoker,
        int priority
    )> _listeners = new();

    public BattleState BattleState;
    public CommandLog CommandLog { get; }

    public IInteractionService Interaction { get; set; }
    public ExecutionMode Mode { get; private set; } = ExecutionMode.Real;

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
        CommandLog = new(state);
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
    public void EnqueueWithBarrier(IGameEvent action, IValueSpec<bool> predicate = null)
    {
        bool isExternal = !_isProcessing;
        _barrierQueue.Add((action, predicate, isExternal));
    }


    public int MaxEventsPerProcessQueue { get; set; } = 10_000; // разумный дефолт, можно поднять при необходимости

    /// <summary>
    /// Обработать все события в основной очереди.
    /// </summary>
    public void ProcessQueue()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        int processedCount = 0;

        while (_mainQueue.Count > 0)
        {
            if (++processedCount > MaxEventsPerProcessQueue)
            {
                _isProcessing = false;
                throw new InvalidOperationException(
                    $"ProcessQueue exceeded {MaxEventsPerProcessQueue} events in a single run — " +
                    $"likely an infinite Replace/Raise loop (a Replace/After system re-triggering itself). " +
                    $"Last event type: {_mainQueue.First?.Value.Event.GetType().Name}");
            }

            var (action, needsSnapshot) = _mainQueue.First.Value;
            _mainQueue.RemoveFirst();

            // Снапшот — тяжёлая операция для отладки/реплея реального боя.
            // В Preview/Simulation результат всё равно будет отброшен CommandLog.UndoTo — снапшот не нужен.
            if (needsSnapshot && Mode == ExecutionMode.Real)
                StateHistory.Add(new GameStateSnapshot(BattleState));

            int eventCheckpoint = CommandLog.Checkpoint;

            foreach (var phase in PhaseOrder)
            {
                if (action.Status == EventStatus.Cancelled || action.Status == EventStatus.Replaced)
                    break;

                var context = new EventContext(BattleState, action, this, Interaction);
                context.CurrentPhase = phase;

                foreach (var entry in _listeners)
                {
                    if (action.Status == EventStatus.Cancelled || action.Status == EventStatus.Replaced)
                        break;
                    if (!entry.phaseType.IsAssignableFrom(phase)) continue;
                    if (!entry.eventType.IsInstanceOfType(action)) continue;
                    entry.invoker(context);
                }
            }

            // Cancelled — откатываем то, что успело намутироваться вопреки контракту фаз (защитная мера).
            // Replaced — по контракту мутаций быть не должно, но откат не помешает, если контракт нарушат.
            if (action.Status == EventStatus.Cancelled || action.Status == EventStatus.Replaced)
                CommandLog.UndoTo(eventCheckpoint);
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

    private bool CheckBarrierResolved(IGameEvent gameEvent, IValueSpec<bool> predicate)
    {
        if (predicate == null) return true;
        var context = new EventContext(BattleState, gameEvent, this, Interaction);
        var con = new ExecutionContext(context, null);
        return predicate.Resolve(con);
    }


    public IExecutionScope EnterMode(ExecutionMode mode)
    {
        if (_isProcessing)
            throw new InvalidOperationException(
                "Нельзя менять ExecutionMode изнутри уже идущего ProcessQueue — " +
                "смена режима допустима только между прогонами очереди.");

        return new ModeScope(this, previousMode: Mode, newMode: mode, checkpoint: CommandLog.Checkpoint);
    }

    private sealed class ModeScope : IExecutionScope
    {
        private readonly EventQueue _queue;
        private readonly ExecutionMode _previousMode;
        private readonly int _checkpoint;
        private readonly bool _autoRollback;
        private bool _committed;

        public ModeScope(EventQueue queue, ExecutionMode previousMode, ExecutionMode newMode, int checkpoint)
        {
            _queue = queue;
            _previousMode = previousMode;
            _checkpoint = checkpoint;
            _autoRollback = newMode != ExecutionMode.Real;
            _queue.Mode = newMode;
        }

        public void Commit() => _committed = true;

        public void Dispose()
        {
            if (_autoRollback && !_committed)
                _queue.CommandLog.UndoTo(_checkpoint);
            _queue.Mode = _previousMode;
        }
    }
}
