using System;


/// <summary>
/// Исполняет TeleportEvent: мгновенно меняет координаты сущности.
/// </summary>
public class TeleportSystem : IEventListener<TeleportEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 0;
    public GEID SystemId { get; } = GEID.New;

    public void OnEvent(EventContext context, TeleportEvent evt)
    {
        var mover = context.BattleState.GetEntity(evt.MoverId);
        if (mover != null)
        {
            var hexComp = mover.GetComponent<HexComponent>();
            if (hexComp != null)
            {
            context.Mutate<HexComponent>(mover.Id, h =>
                h with { Coordinates = evt.TargetHex });
            }
        }
        evt.Status = EventStatus.Applied;
    }
}
