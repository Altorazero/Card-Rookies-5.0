using System;
using System.Collections.Generic;

[Serializable]
public class AllSelector : ISelector<IEntity>
{
    public SelectionResult<IEntity> Select(IEnumerable<IEntity> candidates, ExecutionContext context)
    {
        return SelectionResult<IEntity>.Complete(candidates);
    }
}
