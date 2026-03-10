using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class HealthSystem :
    IEventListener<HealEvent, IModifyPhaseEvent>,
    IEventListener<SingleDamageEvent, IApplyPhaseEvent>,
    ISBAListener

{
    public Geid SystemId { get; } = Geid.New;

    public int Priority { get; } = 100;
    void IEventListener<SingleDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, SingleDamageEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;
        var targetEntity = context.BattleState.GetEntity(tgt);
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                healthComp.CurrentHealth -= evt.DamageAmount;
                Debug.Log($"Entity {targetEntity.Id} took {evt.DamageAmount} damage. Current health: {healthComp.CurrentHealth}");
            }
            else
            {
                Debug.LogWarning($"Entity {targetEntity.Id} has no HealthComponent.");
            }
        }
    }

    void IEventListener<HealEvent, IModifyPhaseEvent>.OnEvent(EventContext context, HealEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;
        var targetEntity = context.BattleState.GetEntity(tgt);
        if (targetEntity != null)
        {
            var healthComp = targetEntity.GetComponent<HealthComponent>();
            if (healthComp != null)
            {
                healthComp.CurrentHealth += evt.HealAmount;
                Debug.Log($"Entity {targetEntity.Id} healed for {evt.HealAmount}. Current health: {healthComp.CurrentHealth}");
            }
            else
            {
                Debug.LogWarning($"Entity {targetEntity.Id} has no HealthComponent.");
            }
        }
    }

    // Новый SBA-слушатель (вызов из диспетчера в фазе ISBAEvent)
    void ISBAListener.OnSBA(EventContext context)
    {
        var aliveEntities = context.BattleState.Entities.Values;
        var toRemove = new List<Geid>();

        // Собираем идентификаторы сущностей для удаления вне цикла перечисления
        foreach (var entity in aliveEntities)
        {
            var healthComp = entity.GetComponent<HealthComponent>();
            if (healthComp != null && healthComp.CurrentHealth <= 0)
            {
                Debug.Log($"Entity {entity.Id} has died.");
                toRemove.Add(entity.Id);
            }
        }

        // Удаляем вне перечисления коллекции, чтобы избежать InvalidOperationException
        foreach (var id in toRemove)
        {
            context.BattleState.RemoveEntity(id);
        }
    }
}