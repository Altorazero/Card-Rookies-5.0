using System.Collections.Generic;

public interface ITargetSelector
{
    /// <summary>
    /// Выбирает цели из доступных кандидатов.
    /// Может выбросить TargetSelectionFailed.
    /// </summary>
    IReadOnlyList<Geid> SelectTarget(EventContext context, IReadOnlyList<Geid> candidates);
}