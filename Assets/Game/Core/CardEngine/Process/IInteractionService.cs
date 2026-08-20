using System.Collections.Generic;

public interface IInteractionService
{
    void RequestTargetSelection<T>(ISelectionExecution<T> execution);
}
