public interface ITargetFilter
{
    bool IsTargetValid(Geid target, EventContext context);
}