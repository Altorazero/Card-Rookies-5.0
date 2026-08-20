using System;
using System.Collections.Generic;

[Serializable]
public class HumanChoiceSelector<T> : ISelector<T>
{
    public int MinCount = 1;
    public int MaxCount = 1;

    public SelectionResult<T> Select(IEnumerable<T> candidates, ExecutionContext context)
    {
        var selectionEx = new SelectionExecution<T>(candidates, MinCount, MaxCount);
        return SelectionResult<T>.Pending(selectionEx);
    }
}
