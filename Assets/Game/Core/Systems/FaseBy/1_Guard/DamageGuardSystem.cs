public sealed class DamageGuardSystem : IEventListener<DamageEvent, IGuardPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        var target = evt.GetFirstSubject(Role.Target);
        if (target == null || !target.HasComponent<HealthComponent>())
        {
            evt.Status = EventStatus.Cancelled;
            return;
        }

        // Пример "закона боя": мёртвых нельзя бить повторно.
        var health = target.GetComponent<HealthComponent>();
        if (health.Current <= 0)
            evt.Status = EventStatus.Cancelled;
    }
}