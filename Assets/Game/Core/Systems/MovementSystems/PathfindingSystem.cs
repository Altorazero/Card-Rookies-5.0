using System.Collections.Generic;

/// <summary>
/// Перехватывает PathMoveEvent.
/// Если путь найден, отменяет PathMoveEvent и кидает в очередь последовательность StepMoveEvent.
/// </summary>
public class PathfindingSystem : IEventListener<PathMoveEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 0;
    public GEID SystemId { get; } = GEID.New;

    public void OnEvent(EventContext context, PathMoveEvent evt)
    {
        var mover = context.BattleState.GetEntity(evt.MoverId);
        if (mover == null)
        {
            evt.Status = EventStatus.Fizzled;
            return;
        }

        var hexComp = mover.GetComponent<HexComponent>();
        if (hexComp == null)
        {
            evt.Status = EventStatus.Fizzled;
            return;
        }

        // Ищем путь от текущей позиции до точки назначения (с учетом высот и преград)
        var path = Pathfinder.FindPath(context.BattleState, hexComp.Coordinates, evt.Destination);

        if (path != null && path.Count > 0)
        {
            // Путь найден! Превращаем его в цепочку шагов и кидаем в диспетчер
            foreach (var stepHex in path)
            {
                var stepEvent = new StepMoveEvent(SystemId, evt.MoverId, stepHex);
                context.Dispatcher.Enqueue(stepEvent);
            }
        }
        else
        {
            // Путь не найден (или заблокирован)
            // (Здесь можно было бы кинуть UI-уведомление "Cannot reach destination")
        }

        // Само событие "Намерения" считается исполненным (оно породило шаги)
        evt.Status = EventStatus.Applied;
    }
}
