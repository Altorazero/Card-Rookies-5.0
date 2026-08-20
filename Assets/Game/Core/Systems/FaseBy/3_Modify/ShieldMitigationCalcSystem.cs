using System;

public sealed class ShieldMitigationCalcSystem : IEventListener<DamageEvent, IModifyPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 10;

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        var target = evt.GetFirstSubject(Role.Target);
        var shield = target.GetComponent<ShieldComponent>();
        if (shield == null || shield.Value <= 0) return;

        int absorbed = Math.Min(shield.Value, evt.Amount);

        evt.Amount -= absorbed;
    }
}