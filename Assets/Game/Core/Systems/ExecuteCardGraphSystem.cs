using System;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteCardGraphSystem : IEventListener<ExecuteCardGraphEvent, IApplyPhaseEvent>
{
    /// <summary>
    /// Максимальное количество посещений узлов за одно выполнение карты.
    /// Защита от бесконечных циклов в цикличных графах.
    /// </summary>
    private const int MaxNodeVisits = 1000;

    public int Priority { get; } = 10;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<ExecuteCardGraphEvent, IApplyPhaseEvent>.OnEvent(EventContext context, ExecuteCardGraphEvent evt)
    {
        var cardGraphNode = evt.CardGraphNode;
        if (cardGraphNode == null)
        {
            Debug.LogWarning($"PlayCardEvent: CardGraphNode is null for event {evt.Id}");
            evt.Status = EventStatus.Fizzled;
            return;
        }
        try
        {
            // Обход графа — может быть цикличным. Защита через MaxNodeVisits.
            int totalVisits = 0;
            var queue = new Queue<CardGraphNode>();
            queue.Enqueue(cardGraphNode);

            while (queue.Count > 0)
            {
                if (totalVisits >= MaxNodeVisits)
                {
                    Debug.LogWarning($"[ExecuteCardGraphSystem] Reached max node visits ({MaxNodeVisits}) for event {evt.Id}. Stopping graph traversal.");
                    break;
                }

                var node = queue.Dequeue();
                totalVisits++;

                // Выполняем все действия, хранящиеся в этом узле
                foreach (var action in node.Events)
                {
                    context.Dispatcher.Enqueue(action);
                }

                // Для каждого связанного узла проверяем условие перехода
                foreach (var (targetNode, condition) in node.TiedNodes)
                {
                    var conditionMet = condition == null || condition.Evaluate(context);
                    if (conditionMet)
                        queue.Enqueue(targetNode);
                }
            }

            evt.Status = EventStatus.Applied;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error executing CardGraphNode for PlayCardEvent {evt.Id}: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
            evt.Status = EventStatus.Fizzled;
        }
    }
}
