using UnityEngine;

/// <summary>
/// Система проверки разыгрывания карты.
/// Guard-фаза: отменяет <see cref="PlayCardCheckEvent"/>, если у кастера не хватает ресурсов
/// (мана, энергия) — результат <c>Cancelled</c>.
/// After-фаза: если проверка прошла успешно, создаёт <see cref="PlayCardEvent"/> и ставит
/// его в очередь; сохраняет ссылку в <see cref="PlayCardCheckEvent.SpawnedPlayCardEvent"/>.
/// </summary>
public class PlayCardCheckSystem :
    IEventListener<PlayCardCheckEvent, IGuardPhaseEvent>,
    IEventListener<PlayCardCheckEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 5;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<PlayCardCheckEvent, IGuardPhaseEvent>.OnEvent(EventContext context, PlayCardCheckEvent evt)
    {
        var entity = context.BattleState.GetEntity(evt.CasterId);
        if (entity == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"[PlayCardCheckSystem] Caster {evt.CasterId} not found. Cancelling.");
            return;
        }

        if (evt.Card.ManaCost > 0)
        {
            var mana = entity.GetComponent<ManaComponent>();
            if (mana == null || mana.CurrentMana < evt.Card.ManaCost)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"[PlayCardCheckSystem] Insufficient mana ({mana?.CurrentMana ?? 0}/{evt.Card.ManaCost}). Cancelled.");
                return;
            }
        }

        if (evt.Card.EnergyCost > 0)
        {
            var energy = entity.GetComponent<EnergyComponent>();
            if (energy == null || energy.CurrentEnergy < evt.Card.EnergyCost)
            {
                evt.Status = EventStatus.Cancelled;
                Debug.Log($"[PlayCardCheckSystem] Insufficient energy ({energy?.CurrentEnergy ?? 0}/{evt.Card.EnergyCost}). Cancelled.");
                return;
            }
        }
    }

    void IEventListener<PlayCardCheckEvent, IAfterPhaseEvent>.OnEvent(EventContext context, PlayCardCheckEvent evt)
    {
        if (evt.Status == EventStatus.Cancelled || evt.Status == EventStatus.Fizzled)
            return;

        var playEvent = new PlayCardEvent(evt.SystemSourceId, evt.Card, evt.CasterId, evt.SelectedTargets);
        evt.SpawnedPlayCardEvent = playEvent;
        context.Dispatcher.Enqueue(playEvent);
        evt.Status = EventStatus.Applied;
    }
}
