using System.Collections.Generic;

/// <summary>
/// Спецификация таргетинга — список последовательных шагов пайплайна.
///
/// Система таргетинга исполняет шаги по очереди через <see cref="TargetingContext"/>.
/// Типичная структура одного «блока»:
///   AllEntitiesPool → FilterStep → SorterStep → TakeSorter → ExitConditionStep
///
/// Пайплайн можно повторять произвольно:
///   Pool → Filter → Sort → Exit → Pool → Filter → Sort → Exit → …
/// что позволяет реализовать любые условия выбора целей без изменения системы.
/// </summary>
public interface ITargetingSpec
{
    /// <summary>Уникальный ID спецификации (для отладки).</summary>
    Geid Id { get; }

    /// <summary>Человекочитаемое описание.</summary>
    string Description { get; set; }

    /// <summary>Роль, под которой найденные цели записываются в Subjects события.</summary>
    SubjectRole TargetRole { get; set; }

    /// <summary>Шаги пайплайна таргетинга.</summary>
    IReadOnlyList<ITargetingStep> Steps { get; }
}

/// <summary>
/// Стандартная реализация <see cref="ITargetingSpec"/>.
/// Используйте fluent-метод <see cref="AddStep"/> для построения пайплайна.
/// </summary>
public class TargetingSpec : ITargetingSpec
{
    private readonly List<ITargetingStep> _steps = new();

    public Geid Id { get; } = Geid.New;
    public string Description { get; set; }
    public SubjectRole TargetRole { get; set; } = SubjectRole.Target;
    public IReadOnlyList<ITargetingStep> Steps => _steps;

    /// <summary>
    /// Добавляет шаг в пайплайн. Возвращает this для fluent-синтаксиса.
    /// </summary>
    public TargetingSpec AddStep(ITargetingStep step)
    {
        if (step != null)
            _steps.Add(step);
        return this;
    }
}

/// <summary>
/// Тип таргетинга (используется в ICardTargeting и для семантической маркировки спеков).
/// </summary>
public enum TargetingType
{
    None,
    Entity,
    Area,
    Direction,
    Projectile
}
