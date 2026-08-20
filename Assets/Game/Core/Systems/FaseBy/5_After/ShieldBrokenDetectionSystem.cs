using UnityEngine;

public sealed class ShieldBrokenDetectionSystem : IEventListener<DamageEvent, IAfterPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        int valueBefore = evt.Scratch.GetOrDefault(BuiltinScratchKeys.ShieldValueBeforeApply);
        if (valueBefore <= 0) return; // щита не было Ч разрушать нечего

        var target = evt.GetFirstSubject(Role.Target);
        var shieldAfter = target.GetComponent<ShieldComponent>();

        bool brokenNow = shieldAfter == null || shieldAfter.Value <= 0;
        if (brokenNow)
            Debug.LogWarning($"Entity {target.Id} has it's shield broken.");

        // context.Raise(new ShieldBrokenEvent(target, causedBy: evt));
    }
}