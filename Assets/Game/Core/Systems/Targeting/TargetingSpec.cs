using System.Collections.Generic;

public interface ITargetingSpec
{
    Geid Id { get; set; }
    string Description { get; set; }
    TargetingType Type { get; set; }
    
    /// <summary>
    /// Фильтр для определения валидных целей
    /// </summary>
    ITargetFilter TargetFilter { get; set; }
    
    /// <summary>
    /// Селектор для выбора целей из кандидатов
    /// </summary>
    ITargetSelector Selector { get; set; }
    
    /// <summary>
    /// Минимальное количество целей (если не удается выбрать - fizzle)
    /// </summary>
    int MinTargets { get; set; }
    
    /// <summary>
    /// Максимальное количество целей
    /// </summary>
    int MaxTargets { get; set; }
    
    /// <summary>
    /// Роль, которую получат выбранные цели в SubjectsList
    /// </summary>
    SubjectRole TargetRole { get; set; }
    
    /// <summary>
    /// Источник кандидатов для таргетинга (null = все сущности на поле боя)
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
