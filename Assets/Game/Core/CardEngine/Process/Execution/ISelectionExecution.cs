using System;
using System.Collections.Generic;

public interface ISelectionExecution<T>
{
    IEnumerable<T> Candidates { get; }
    int MinCount { get; }
    int MaxCount { get; }
    event Action<IEnumerable<T>> OnComplete;
    event Action OnCancel;
    event Action<IEnumerable<T>> OnHoverChanged; // промежуточное наведение, не завершает выбор
    void Complete(IEnumerable<T> selection);
    void Cancel();
    void NotifyHover(IEnumerable<T> candidates); // вызывается UI при наведении
}