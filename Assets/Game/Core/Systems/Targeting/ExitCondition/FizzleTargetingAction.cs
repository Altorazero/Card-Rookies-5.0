/// <summary>
/// Действие: переводит событие в статус <see cref="EventStatus.Fizzled"/> и останавливает пайплайн.
/// Цели НЕ записываются в Subjects.
/// </summary>
public class FizzleTargetingAction : ITargetingAction
{
    public void Execute(TargetingContext context)
    {
        context.TargetingEvent.Status = EventStatus.Fizzled;
        context.Stopped = true;
    }
}
