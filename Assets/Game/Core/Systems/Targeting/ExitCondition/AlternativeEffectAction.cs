using System;
using UnityEngine;

/// <summary>
/// Действие: отменяет оригинальное событие, создаёт альтернативное через фабрику и добавляет
/// его в начало очереди событий. Останавливает пайплайн.
///
/// Вся логика альтернативного эффекта инкапсулирована в фабрике;
/// система таргетинга лишь вызывает её и ставит результат в очередь.
/// </summary>
public class AlternativeEffectAction : ITargetingAction
{
    public Func<EventContext, IGameEvent> Factory { get; }

    public AlternativeEffectAction(Func<EventContext, IGameEvent> factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public void Execute(TargetingContext context)
    {
        context.TargetingEvent.Status = EventStatus.Cancelled;
        context.Stopped = true;

        var altEvent = Factory(context.EventContext);
        if (altEvent != null)
        {
            Debug.Log($"[AlternativeEffectAction] Dispatching alternative event {altEvent.Id} " +
                      $"for original event {context.TargetingEvent.Id}.");
            context.EventContext.Dispatcher.Enqueue(altEvent, atFront: true);
        }
        else
        {
            Debug.LogWarning("[AlternativeEffectAction] Factory returned null; alternative event not dispatched.");
        }
    }
}
