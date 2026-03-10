using System;
using System.Collections.Generic;

/// <summary>
/// Составной фильтр: принимает цель если ХОТЯ БЫ ОДИН внутренний фильтр вернул true (OR).
/// Поддерживает произвольную вложенность.
/// </summary>
public class OrTargetFilter : ITargetFilter
{
    public IReadOnlyList<ITargetFilter> Filters { get; }

    public OrTargetFilter(params ITargetFilter[] filters) : this((IEnumerable<ITargetFilter>)filters) { }

    public OrTargetFilter(IEnumerable<ITargetFilter> filters)
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
            if (f.IsTargetValid(target, context)) return true;
        return false;
    }
}
