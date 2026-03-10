using System;

/// <summary>
/// Инвертирующий фильтр: принимает цель только если внутренний фильтр вернул false (NOT).
/// </summary>
public class NotTargetFilter : ITargetFilter
{
    public ITargetFilter Inner { get; }

    public NotTargetFilter(ITargetFilter inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public bool IsTargetValid(Geid target, EventContext context) => !Inner.IsTargetValid(target, context);
}
