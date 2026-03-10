using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class RandomMovementSystem : IEventListener<RandomMovementEvent, IGuardPhaseEvent>,
    IEventListener<RandomMovementEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;

    public Geid SystemId { get; } = Geid.New;
    void IEventListener<RandomMovementEvent, IGuardPhaseEvent>.OnEvent(EventContext context, RandomMovementEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        //var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;

        var entity = context.BattleState.GetEntity(tgt);
        var positionComp = entity?.GetComponent<HexComponent>();

        if (entity == null || positionComp == null)
        {
            Debug.LogWarning($"RandomMovementEvent: Entity {tgt} not found or has no HexComponent.");
            evt.Status = EventStatus.Cancelled;
            return;
        }
        
    }

    void IEventListener<RandomMovementEvent, IApplyPhaseEvent>.OnEvent(EventContext context, RandomMovementEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        //var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;

        var entity = context.BattleState.GetEntity(tgt);
        var positionComp = entity?.GetComponent<HexComponent>();

        var radius = Math.Max(0, evt.Radius);
        var rng = context.BattleState.Rng;

        // Собираем оффсеты в кубовой системе и конвертируем в аксиальные (q, r)
        var offsets = new List<HexCoordinates>();
        for (int x = -radius; x <= radius; x++)
        {
            int yMin = Math.Max(-radius, -x - radius);
            int yMax = Math.Min(radius, -x + radius);
            for (int y = yMin; y <= yMax; y++)
            {
                int z = -x - y;
                // в нашей структуре HexCoordinates: Q = x, R = z
                offsets.Add(new HexCoordinates(x, z));
            }
        }

        // Исключаем текущую клетку, если есть альтернативы (чтобы реально перемещался)
        var origin = positionComp.Coordinates;
        if (offsets.Count > 1)
            offsets.RemoveAll(o => o.Q == 0 && o.R == 0);

        if (offsets.Count == 0)
        {
            Debug.LogWarning($"RandomMovementEvent: no available offsets for radius {radius}");
            return;
        }

        var chosenOffset = rng.PickOne(offsets);
        var newPosition = origin + chosenOffset;

        var moveEvent = new MoveEntityEvent(evt.SystemSourceId, tgt, newPosition);
        context.Dispatcher.Enqueue(moveEvent, true);

        Debug.Log($"Entity {entity.Id} scheduled to move from {origin} to {newPosition} (random offset {chosenOffset})");
    }
}