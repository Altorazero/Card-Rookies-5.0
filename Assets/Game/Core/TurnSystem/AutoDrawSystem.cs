using UnityEngine;

/// <summary>
/// Система автоматического добора карт.
/// Срабатывает на BattleStartEvent (добор для всех) и TurnStartEvent (добор для сущностей команды).
/// 
/// При BattleStartEvent: выполняет AutoDraw() для всех сущностей с HandComponent.
/// При TurnStartEvent: выполняет AutoDraw() для сущностей из команды, чей ход начинается,
///   НО только если HasDrawnInitial уже установлен (иначе — это первый ход, и добор уже был в начале боя).
/// </summary>
public class AutoDrawSystem :
    IEventListener<BattleStartEvent, IApplyPhaseEvent>,
    IEventListener<TurnStartEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 200;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<BattleStartEvent, IApplyPhaseEvent>.OnEvent(EventContext context, BattleStartEvent evt)
    {
        foreach (var entity in context.BattleState.Entities.Values)
        {
            var hand = entity.GetComponent<HandComponent>();
            if (hand == null) continue;

            hand.AutoDraw();
            hand.HasDrawnInitial = true;
            Debug.Log($"[AutoDrawSystem] Battle start: entity {entity.Id} drew {hand.AutoDrawCount} card(s). Hand size: {hand.Count}");
        }
        evt.Status = EventStatus.Applied;
    }

    void IEventListener<TurnStartEvent, IApplyPhaseEvent>.OnEvent(EventContext context, TurnStartEvent evt)
    {
        var teamId = evt.TeamId;

        foreach (var entity in context.BattleState.Entities.Values)
        {
            var team = entity.GetComponent<TeamComponent>();
            if (team == null || team.TeamId != teamId) continue;

            var hand = entity.GetComponent<HandComponent>();
            if (hand == null) continue;

            if (!hand.HasDrawnInitial)
            {
                // Первый ход для этой сущности — начальный добор уже был при BattleStartEvent
                hand.HasDrawnInitial = true;
                continue;
            }

            hand.AutoDraw();
            Debug.Log($"[AutoDrawSystem] Turn start: entity {entity.Id} drew {hand.AutoDrawCount} card(s). Hand size: {hand.Count}");
        }
        evt.Status = EventStatus.Applied;
    }
}
