public class RandomDamageSystem : IEventListener<RandomDamageEvent, IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;

    void IEventListener<RandomDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, RandomDamageEvent evt)
    {
        var tgt = evt.GetFirstSubject(SubjectRole.Target);
        var src = evt.GetFirstSubject(SubjectRole.Source);
        context.Dispatcher.Enqueue(
            new SingleDamageEvent(evt.SystemSourceId, src, tgt,
                context.BattleState.Rng.NextInt(evt.LowerBound, evt.UpperBound + 1)),
            true);
    }
}
