using UnityEngine;

public class VampSystem : IEventListener<DamageEvent, IAfterPhaseEvent>
{
    public int Priority { get; } = 50;
    public GEID SystemId { get; } = GEID.New;

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        var srcId = evt.GetFirstSubject(Role.Source);
        var sourceEntity = srcId;
        if (sourceEntity == null) return;

        var vampComp = sourceEntity.GetComponent<VampComponent>();
        if (vampComp != null)
        {
            int vampAmount = Mathf.FloorToInt(evt.Amount * vampComp.VampLevel);
            if (vampAmount > 0)
            {
                var h = new HealEvent(evt.SystemSourceId, vampAmount);
                h.Subjects[Role.Source].Add(srcId);
                h.Subjects[Role.Target].Add(srcId);
                context.Dispatcher.Enqueue(h, true);
            }
        }
    }
}
