/// <summary>
/// Исполняет атомарный шаг (сдвигает координаты).
/// На фазе Guard: проверяет, не заняли ли гекс пока мы шли (ведь кто-то мог встать туда перед нами).
/// На фазе Apply: меняет координаты.
/// На фазе After: (можно добавить логику для рассылки триггеров ловушек).
/// </summary>
public class StepMoveSystem : 
    IEventListener<StepMoveEvent, IGuardPhaseEvent>,
    IEventListener<StepMoveEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 0;
    public GEID SystemId { get; } = GEID.New;

    public void OnEvent(EventContext context, StepMoveEvent evt)
    {
        if (context.CurrentPhase == typeof(IGuardPhaseEvent))
        {
            // Проверяем актуальность: вдруг кто-то уже наступил на наш целевой гекс?
            // Или гекс разрушили (упал в пропасть)?
            if (context.BattleState.GetTileAtHex(evt.TargetHex) == null || 
                context.BattleState.GetOccupantAtHex(evt.TargetHex) != null)
            {
                evt.Status = EventStatus.Fizzled; // Шаг прерван
            }
        }
        else if (context.CurrentPhase == typeof(IApplyPhaseEvent))
        {
            var mover = context.BattleState.GetEntity(evt.MoverId);
            if (mover != null)
            {
                var hexComp = mover.GetComponent<HexComponent>();
                if (hexComp != null)
                {
                    evt.PreviousHex = hexComp.Coordinates;
                    context.Mutate<HexComponent>(mover.Id, h =>
                        h with { Coordinates = evt.TargetHex });
                }
            }
            evt.Status = EventStatus.Applied;
        }
    }
}
