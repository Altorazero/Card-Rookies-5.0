using System;
using System.Collections.Generic;

public interface ISuspendPoint
{
    GEID NodeId { get; }
    event Action Resolved;
    event Action Cancelled;
    void Cancel();
}

public sealed class SelectionSuspendPoint<T> : ISuspendPoint
{
    public GEID NodeId { get; }
    public ISelectionExecution<T> Selection { get; }
    public IEnumerable<T> Result { get; private set; }

    public event Action Resolved;
    public event Action Cancelled;
    public event Action<IEnumerable<T>> CandidatesPreviewed;

    public SelectionSuspendPoint(GEID nodeId, ISelectionExecution<T> selection)
    {
        NodeId = nodeId;
        Selection = selection;
        selection.OnComplete += OnComplete;
        selection.OnCancel += () => Cancelled?.Invoke();
        selection.OnHoverChanged += candidates => CandidatesPreviewed?.Invoke(candidates);
    }

    private void OnComplete(IEnumerable<T> value)
    {
        Result = value;
        Resolved?.Invoke();
    }

    public void Cancel() => Selection.Cancel();
}