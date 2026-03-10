public interface ITargetingSpec
{
    Geid Id { get; set; }
    string Description { get; set; }
    TargetingType Type { get; set; }

    /// <summary>
    /// Фильтр для определения допустимых целей.
    /// </summary>
    ITargetFilter TargetFilter { get; set; }

    /// <summary>
    /// Селектор для выбора целей из допустимых.
    /// </summary>
    ITargetSelector Selector { get; set; }

    /// <summary>
    /// Минимальное количество целей (если не наберётся — fizzle).
    /// </summary>
    int MinTargets { get; set; }

    /// <summary>
    /// Максимальное количество целей.
    /// </summary>
    int MaxTargets { get; set; }

    /// <summary>
    /// Роль, под которой выбранные цели добавляются в Subjects.
    /// </summary>
    SubjectRole TargetRole { get; set; }

    /// <summary>
    /// Сущность-источник для таргетинга (null = без фильтра по источнику).
    /// </summary>
    Geid? SourceEntity { get; set; }
}

public class BasicTargetingSpec : ITargetingSpec
{
    public Geid Id { get; set; } = Geid.New;
    public string Description { get; set; }
    public TargetingType Type { get; set; }
    public ITargetFilter TargetFilter { get; set; }
    public ITargetSelector Selector { get; set; }
    public int MinTargets { get; set; } = 1;
    public int MaxTargets { get; set; } = 1;
    public SubjectRole TargetRole { get; set; } = SubjectRole.Target;
    public Geid? SourceEntity { get; set; }
}

public enum TargetingType
{
    None,
    Entity,
    Area,
    Direction,
    Projectile
}
