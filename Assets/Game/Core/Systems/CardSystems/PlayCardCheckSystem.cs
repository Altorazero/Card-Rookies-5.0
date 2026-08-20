using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система проверки возможности разыграть карту (PlayCardCheckEvent).
/// На Guard-фазе выполняет проверку нахождения карты в руке и "сухой" запуск проверки стоимости.
/// На After-фазе извлекает карту из руки, кладет в сброс и ставит в очередь событие PlayCardEvent.
/// </summary>
public class PlayCardCheckSystem :
    IEventListener<PlayCardCheckEvent, IGuardPhaseEvent>,
    IEventListener<PlayCardCheckEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 100;
    public GEID SystemId { get; } = GEID.New;

    void IEventListener<PlayCardCheckEvent, IGuardPhaseEvent>.OnEvent(EventContext context, PlayCardCheckEvent evt)
    {
        var caster = evt.Caster;
        if (caster == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning("[PlayCardCheckSystem] Play cancelled: no caster specified.");
            return;
        }

        // 1. Проверяем, что карта действительно в руке (если у кастера есть HandComponent)
        var hand = caster.GetComponent<HandComponent>();
        if (hand != null)
        {
            bool hasCard = false;
            foreach (var card in hand.Cards)
            {
                if (card == evt.Card)
                {
                    hasCard = true;
                    break;
                }
            }

            if (!hasCard)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.LogWarning($"[PlayCardCheckSystem] Play cancelled: card {evt.Card.Id} is not in caster's hand.");
                return;
            }
        }
    }

    void IEventListener<PlayCardCheckEvent, IAfterPhaseEvent>.OnEvent(EventContext context, PlayCardCheckEvent evt)
    {
        if (evt.Status == EventStatus.Cancelled) return;

        var caster = evt.Caster;

        // 1. Перемещаем карту из руки в сброс
        var hand = caster.GetComponent<HandComponent>();
        var discard = caster.GetComponent<DiscardComponent>();

        if (hand != null)
        {
            hand.Remove(evt.Card);
        }

        if (discard != null)
        {
            discard.Add(evt.Card);
        }

        // 2. Спавним PlayCardEvent и закидываем в очередь событий
        var playEvent = new PlayCardEvent(SystemId, evt.Card, caster);
        evt.SpawnedPlayCardEvent = playEvent;
        context.Dispatcher.Enqueue(playEvent);

        evt.Status = EventStatus.Applied;
    }
}
