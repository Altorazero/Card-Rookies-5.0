using System;
using System.Collections.Generic;

[Serializable]
public class AllEntitiesProvider<IEntity> : ICandidateProvider<IEntity>
{
    public IEnumerable<IEntity> GetValues(ExecutionContext context)
    {
        return (IEnumerable<IEntity>)context.EventContext.BattleState.Entities.Values;
    }
}
