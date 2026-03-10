/// <summary>
/// Действие: переводит событие в статус <see cref="EventStatus.Cancelled"/> и останавливает пайплайн.
/// Цели НЕ записываются в Subjects.
/// </summary>
public class CancelTargetingAction : ITargetingAction
{
    public void Execute(TargetingContext context)
    {
        context.TargetingEvent.Status = EventStatus.Cancelled;
        context.Stopped = true;
    }
}
