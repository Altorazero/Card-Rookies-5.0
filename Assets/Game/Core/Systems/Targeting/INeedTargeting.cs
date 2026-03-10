public interface INeedTargeting : IHaveSubjects
{
    public ITargetingSpec TargetingSpec { get; set; }
}