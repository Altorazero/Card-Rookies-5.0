using System;
using System.Collections.Generic;

public sealed class SelectionExecution<T> : ISelectionExecution<T>
{
    public IEnumerable<T> Candidates { get; }
    public int MinCount { get; }
    public int MaxCount { get; }

    public event Action<IEnumerable<T>> OnComplete;
    public event Action OnCancel;
    public event Action<IEnumerable<T>> OnHoverChanged;

    public SelectionExecution(IEnumerable<T> candidates, int minCount, int maxCount)
    {
        Candidates = candidates;
        MinCount = minCount;
        MaxCount = maxCount;
    }

    public void Complete(IEnumerable<T> selection)
    {
        OnComplete?.Invoke(selection);
    }

    public void Cancel()
    {
        OnCancel?.Invoke();
    }

    public void NotifyHover(IEnumerable<T> candidates)
    {
        throw new NotImplementedException();
    }
}
