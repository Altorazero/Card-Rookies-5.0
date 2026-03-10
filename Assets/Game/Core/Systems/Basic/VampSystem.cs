using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public class VampSystem : IEventListener<SingleDamageEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 100;

    public Geid SystemId { get; } = Geid.New;
    void IEventListener<SingleDamageEvent, IAfterPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {

        var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;
        var healthComp = context.BattleState.GetEntity(src)?.GetComponent<HealthComponent>();
        var vampComp = context.BattleState.GetEntity(src)?.GetComponent<VampComponent>();
        if (vampComp != null && healthComp != null)
        {
            int healAmount = (int)(evt.DamageAmount * vampComp.VampLevel);
            context.Dispatcher.Enqueue(new HealEvent(src, src, src, healAmount), true);
            Debug.Log($"Entity {src} healed for {healAmount} due to vampirism.");
        }

    }
}