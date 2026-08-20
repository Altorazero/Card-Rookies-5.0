using System.Collections.Generic;

public readonly struct SelectionResult<T>
{
    public bool IsCompleted { get; }
    public IEnumerable<T> Value { get; }
    public ISelectionExecution<T> Execution { get; }

    private SelectionResult(bool isCompleted, IEnumerable<T> value, ISelectionExecution<T> execution)
    {
        IsCompleted = isCompleted;
        Value = value;
        Execution = execution;
    }

    public static SelectionResult<T> Complete(IEnumerable<T> value) 
        => new SelectionResult<T>(true, value, null);

    public static SelectionResult<T> Pending(ISelectionExecution<T> execution) 
        => new SelectionResult<T>(false, null, execution);
}
