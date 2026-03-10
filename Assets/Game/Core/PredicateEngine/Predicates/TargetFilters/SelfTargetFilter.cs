/// <summary>
/// Фильтр: принимает только сущность-источник события (Self).
/// </summary>
public class SelfTargetFilter : ITargetFilter
{
    public bool IsTargetValid(Geid target, EventContext context)
    {
        if (context?.Event == null)
            return false;

        if (context.Event is IHaveSubjects eventWithSubjects)
        {
            var source = eventWithSubjects.GetFirstSubject(SubjectRole.Source);
            return source != Geid.Empty && source.Equals(target);
        }
        return false;
    }
}
