using UnityEngine;

public class DamageSystem :
    IEventListener<DamageEvent, IGuardPhaseEvent>,
    IEventListener<DamageEvent, IModifyPhaseEvent>,
    IEventListener<DamageEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 10;
    public GEID SystemId { get; } = GEID.New;

    void IEventListener<DamageEvent, IGuardPhaseEvent>.OnEvent(EventContext context, DamageEvent evt)
    {
        if (evt.Amount <= 0)
        {
            evt.Status = EventStatus.Cancelled;
            return;
        }
        if (evt.GetFirstSubject(Role.Source) == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"DamageEvent {evt.Id} cancelled: no source specified.");
            return;
        }
        var tgt = evt.GetFirstSubject(Role.Target);
        if (tgt == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"DamageEvent {evt.Id} cancelled: no target specified.");
            return;
        }
        if (tgt?.GetComponent<HealthComponent>() == null)
        {
            evt.Status = EventStatus.Cancelled;
            Debug.LogWarning($"DamageEvent {evt.Id} cancelled: target has no HealthComponent.");
        }
    }

    void IEventListener<DamageEvent, IModifyPhaseEvent>.OnEvent(EventContext context, DamageEvent evt)
    {
        var srcId = evt.GetFirstSubject(Role.Source);
        var pwr = srcId?.GetComponent<PowerComponent>()?.PowerLevel;
        if (pwr != null)
            evt.Amount += pwr.Value;
    }

    void IEventListener<DamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, DamageEvent evt)
    {
    }
}
