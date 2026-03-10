using UnityEngine;

public class VampSystem : IEventListener<SingleDamageEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<SingleDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var srcId = evt.GetFirstSubject(SubjectRole.Source);
        var healthComp = context.BattleState.GetEntity(srcId)?.GetComponent<HealthComponent>();
        var vampComp = context.BattleState.GetEntity(srcId)?.GetComponent<VampComponent>();
        if (vampComp != null && healthComp != null)
        {
            int healAmount = (int)(evt.DamageAmount * vampComp.VampLevel);
            context.Dispatcher.Enqueue(new HealEvent(srcId, srcId, srcId, healAmount), true);
            Debug.Log($"Entity {srcId} healed for {healAmount} due to vampirism.");
        }
    }
}
