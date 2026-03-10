using System;
using System.Collections.Generic;
using System.Linq;

// Generic listener signature (общие поля)
public interface IBaseEventListener
{
    public Geid SystemId { get; }
    int Priority { get; }
}
public interface IEventListener<TEvent, TPhase> : IBaseEventListener
    where TEvent : IGameEvent
    where TPhase : IPhaseEvent
{
    void OnEvent(EventContext context, TEvent evt);
}

// Новый вид слушателя для SBA-фазы
public interface ISBAListener : IBaseEventListener
{
    void OnSBA(EventContext context);
}


public class EventDispatcher
{
    // очередь основных действий
    private readonly LinkedList<IGameEvent> _mainQueue = new();
    private readonly List<IGameEvent> _barrierQueue = new();

    // хранилище слушателей (listener, eventType, phaseType, invoker, priority)
    private readonly List<(
        object listener,
        Type eventType,
        Type phaseType,
        Action<EventContext> invoker,
        int priority
    )> _listeners = new();

/*    public TargetingInterpreter TargetingInterpreter { get; set; }
*/    public  BattleState BattleState;
    private static readonly Type[] PhaseOrder = new Type[]
    {
        typeof(IGuardPhaseEvent),
        typeof(IReplacePhaseEvent),
        typeof(IModifyPhaseEvent),
        typeof(ITargetResolvePhaseEvent),
        typeof(IApplyPhaseEvent),
        typeof(IAfterPhaseEvent),
        typeof(ISBAEvent),
    };

    public EventDispatcher(BattleState state)
    {
        BattleState = state;
    }

    // Универсальный метод подписки - автоматически находит все интерфейсы слушателей
    public void Subscribe(IBaseEventListener system)
    {
        if (system == null) return;

        var systemType = system.GetType();
        var interfaces = systemType.GetInterfaces();

        // Проверяем ISBAListener
        if (system is ISBAListener sbaListener)
        {
            SubscribeSBAListener(sbaListener);
        }

        // Проверяем все generic IEventListener<TEvent, TPhase>
        foreach (var iface in interfaces)
        {
            if (!iface.IsGenericType) continue;
            
            var genericDef = iface.GetGenericTypeDefinition();
            if (genericDef != typeof(IEventListener<,>)) continue;

            var typeArgs = iface.GetGenericArguments();
            var eventType = typeArgs[0];
            var phaseType = typeArgs[1];

            // Проверяем, что не дублируем подписку
            if (_listeners.Any(e => ReferenceEquals(e.listener, system) && 
                                   e.eventType == eventType && 
                                   e.phaseType == phaseType))
                continue;

            // Создаём invoker через рефлексию
            var method = iface.GetMethod("OnEvent");
            Action<EventContext> invoker = ctx =>
            {
                // Проверяем тип события - событие уже правильного типа, просто передаём его
                if (eventType.IsInstanceOfType(ctx.Event))
                {
                    method.Invoke(system, new object[] { ctx, ctx.Event });
                }
            };

            _listeners.Add((
                system,
                eventType,
                phaseType,
                invoker,
                system.Priority
            ));
        }

        _listeners.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    // Подписка для generic-слушателей (оставлена для обратной совместимости)
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

        _listeners.Add((
            listener,
            typeof(TEvent),
            typeof(TPhase),
            invoker,
            listener.Priority
        ));

        _listeners.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    // Подписка для SBA-слушателей (внутренний метод)
    private void SubscribeSBAListener(ISBAListener listener)
    {
        if (_listeners.Any(e => ReferenceEquals(e.listener, listener) && e.phaseType == typeof(ISBAEvent)))
            return;

        Action<EventContext> invoker = ctx =>
        {
            listener.OnSBA(ctx);
        };

        _listeners.Add((
            listener,
            typeof(IGameEvent),
            typeof(ISBAEvent),
            invoker,
            listener.Priority
        ));
    }


    // Enqueue — добавление в очередь (atFront опционально)
    public void Enqueue(IGameEvent action, bool atFront = false)
    {
        if (atFront)
            _mainQueue.AddFirst(action);
        else
            _mainQueue.AddLast(action);
    }

    public void EnqueueWithBarrier(IGameEvent action)
    {
        _barrierQueue.Add(action);
    }

    public void ProcessQueue()
    {
        while (_mainQueue.Count > 0)
        {
            var action = _mainQueue.First.Value;
            _mainQueue.RemoveFirst();

            foreach (var phase in PhaseOrder)
            {
                if (action.Status == EventStatus.Cancelled)
                    break;

                var context = new EventContext(BattleState, action, this);

                foreach (var entry in _listeners)
                {
                    if (action.Status == EventStatus.Cancelled)
                        break;

                    // фильтрация по фазе
                    if (!entry.phaseType.IsAssignableFrom(phase))
                        continue;

                    // фильтрация по типу события
                    if (!entry.eventType.IsInstanceOfType(action))
                        continue;

                    entry.invoker(context);
                }
            }
        }

        ProcessBarrierQueue();
    }


    private void ProcessBarrierQueue()
    {
        for (int i = _barrierQueue.Count - 1; i >= 0; i--)
        {
            var action = _barrierQueue[i];
            if (CheckBarrierResolved(action))
            {
                _barrierQueue.RemoveAt(i);
                Enqueue(action);
            }
        }

        if (_mainQueue.Count > 0)
        {
            ProcessQueue();
        }
    }

    private bool CheckBarrierResolved(IGameEvent gameEvent)
    {
        // TODO: логика проверки barrier-ов
        return true;
    }
}