using System.Collections.Generic;

public interface ISelector<T>
{
    SelectionResult<T> Select(IEnumerable<T> candidates, ExecutionContext context);
}
