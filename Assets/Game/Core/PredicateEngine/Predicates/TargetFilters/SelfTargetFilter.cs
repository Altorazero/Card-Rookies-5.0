using System.Linq;

/// <summary>
/// Фильтр, который выбирает только источник события (самого себя)
/// </summary>
public class SelfTargetFilter : ITargetFilter
{
    public bool IsTargetValid(Geid target, EventContext context)
    {
        if (context?.Event == null)
        {
            return false;
        }

        // Получаем источник из SubjectsList текущего события
        if (context.Event is IHaveSubjects eventWithSubjects)
        {
            var source = eventWithSubjects.SubjectsList?
                .FirstOrDefault(s => s.Role == SubjectRole.Source)?.Entity;
            
            return source.HasValue && source.Value.Equals(target);
        }

        return false;
    }
}