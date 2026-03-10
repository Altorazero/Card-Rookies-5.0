using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ExecuteCardGraphSystem : IEventListener<ExecuteCardGraphEvent, IApplyPhaseEvent>
{
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
        // Execute the card graph node and all reachable nodes whose transition conditions are met
        try
        {
            // Заменить стек на очередь и использовать Enqueue/Dequeue для обхода в ширину
            var visited = new HashSet<Geid>();
            var queue = new Queue<CardGraphNode>();

            queue.Enqueue(cardGraphNode);
            visited.Add(cardGraphNode.Id);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                // Execute all actions stored in this node
                foreach (var action in node.Events)
                {
                    context.Dispatcher.Enqueue(action);
                }

                // For each tied node check the transition condition; if met, schedule execution
                foreach (var (targetNode, condition) in node.TiedNodes)
                {
                    var conditionMet = condition == null || condition.Evaluate(context);
                    if (!conditionMet) continue;

                    if (!visited.Contains(targetNode.Id))
                    {
                        visited.Add(targetNode.Id);
                        queue.Enqueue(targetNode);
                    }
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
