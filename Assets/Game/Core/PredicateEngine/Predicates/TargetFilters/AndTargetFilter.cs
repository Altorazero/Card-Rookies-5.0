using System;
using System.Collections.Generic;

/// <summary>
/// Составной фильтр: принимает цель только если ВСЕ внутренние фильтры вернули true (AND).
/// Поддерживает произвольную вложенность.
/// </summary>
public class AndTargetFilter : ITargetFilter
{
    public IReadOnlyList<ITargetFilter> Filters { get; }

    public AndTargetFilter(params ITargetFilter[] filters) : this((IEnumerable<ITargetFilter>)filters) { }

    public AndTargetFilter(IEnumerable<ITargetFilter> filters)
    {
        if (filters == null) throw new ArgumentNullException(nameof(filters));
        var list = new List<ITargetFilter>();
        foreach (var f in filters)
            if (f != null) list.Add(f);
        Filters = list;
    }

    public bool IsTargetValid(Geid target, EventContext context)
    {
        foreach (var f in Filters)
            if (!f.IsTargetValid(target, context)) return false;
        return true;
    }
}
