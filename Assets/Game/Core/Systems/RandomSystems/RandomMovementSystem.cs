using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementSystem :
    IEventListener<RandomMovementEvent, IGuardPhaseEvent>,
    IEventListener<RandomMovementEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<RandomMovementEvent, IGuardPhaseEvent>.OnEvent(EventContext context, RandomMovementEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var entity = context.BattleState.GetEntity(tgt);
        if (entity == null || entity.GetComponent<HexComponent>() == null)
        {
            Debug.LogWarning($"RandomMovementEvent: Entity {tgt} not found or has no HexComponent.");
            evt.Status = EventStatus.Cancelled;
        }
    }

    void IEventListener<RandomMovementEvent, IApplyPhaseEvent>.OnEvent(EventContext context, RandomMovementEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var entity = context.BattleState.GetEntity(tgt);
        var positionComp = entity?.GetComponent<HexComponent>();

        var radius = Math.Max(0, evt.Radius);
        var rng = context.BattleState.Rng;

        // Вычисляем клетки в пределах заданного радиуса в кубических координатах (q, r)
        var offsets = new List<HexCoordinates>();
        for (int x = -radius; x <= radius; x++)
        {
            int yMin = Math.Max(-radius, -x - radius);
            int yMax = Math.Min(radius, -x + radius);
            for (int y = yMin; y <= yMax; y++)
            {
                int z = -x - y;
                offsets.Add(new HexCoordinates(x, z));
            }
        }

        // Исключаем текущую позицию, если есть другие варианты
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
