using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using UnityEditor.Search;
using static UnityEngine.GraphicsBuffer;

public class RandomDamageSystem : IEventListener<RandomDamageEvent,IApplyPhaseEvent>
{
    public int Priority { get; } = 100;
    public Geid SystemId { get; } = Geid.New;
    void IEventListener<RandomDamageEvent, IApplyPhaseEvent>.OnEvent(EventContext context, RandomDamageEvent evt)
    {
        var tgt = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Target).Entity;
        var src = evt.SubjectsList.SingleOrDefault(t => t.Role == SubjectRole.Source).Entity;
        context.Dispatcher.Enqueue(new SingleDamageEvent(evt.SystemSourceId, src, tgt,
            context.BattleState.Rng.NextInt(evt.LowerBond, evt.UpperBond + 1)), true);

    }
}