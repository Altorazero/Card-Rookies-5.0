using System;
using System.Collections.Generic;

/// <summary>
/// Описание правил таргетинга: кого выбирать, как и сколько, что делать при нехватке целей.
/// </summary>
public interface ITargetingSpec
{
    /// <summary>Уникальный ID спецификации (для отладки).</summary>
    Geid Id { get; }

    /// <summary>Человекочитаемое описание.</summary>
    string Description { get; set; }

    /// <summary>Тип таргетинга (Entity, Area, Direction, Projectile, None).</summary>
    TargetingType Type { get; set; }

    /// <summary>
    /// Список фильтров целей с AND-логикой: кандидат принимается только если
    /// все фильтры вернули true. Пустой список — «без фильтрации».
    /// </summary>
    IReadOnlyList<ITargetFilter> Filters { get; }

    /// <summary>Приоритет (порядок сортировки) при выборе из валидных кандидатов.</summary>
    TargetPriority Priority { get; set; }

    /// <summary>Минимальное количество целей для продолжения события.</summary>
    int MinTargets { get; set; }

    /// <summary>
    /// Максимальное количество выбираемых целей.
    /// Используйте <see cref="TargetCount.All"/> для выбора всех валидных кандидатов.
    /// </summary>
    int MaxTargets { get; set; }

    /// <summary>Роль, под которой выбранные цели добавляются в Subjects события.</summary>
    SubjectRole TargetRole { get; set; }

    /// <summary>
    /// Сущность-источник: используется фильтрами (SelfFilter и др.)
    /// и приоритетом Nearest для расчёта дистанции.
    /// </summary>
    Geid? SourceEntity { get; set; }

    /// <summary>Поведение при нехватке целей (меньше MinTargets).</summary>
    InsufficientTargetsBehavior OnInsufficientTargets { get; set; }

    /// <summary>
    /// Фабрика альтернативного события. Вызывается при
    /// <see cref="InsufficientTargetsBehavior.AlternativeEffect"/>.
    /// Если null — событие просто отменяется без создания альтернативы.
    /// Вся логика альтернативного эффекта инкапсулирована в фабрике,
    /// система таргетинга лишь вызывает её и ставит результат в очередь.
    /// </summary>
    Func<EventContext, IGameEvent> AlternativeEffectFactory { get; set; }
}

/// <summary>
/// Вспомогательные константы для задания количества целей.
/// </summary>
public static class TargetCount
{
    /// <summary>Выбрать всех валидных кандидатов без ограничения количества.</summary>
    public const int All = int.MaxValue;

    /// <summary>Явное указание нулевого числа целей.</summary>
    public const int None = 0;
}

/// <summary>
/// Стандартная реализация <see cref="ITargetingSpec"/>.
/// Поддерживает добавление нескольких фильтров через fluent-метод <see cref="AddFilter"/>.
/// </summary>
public class TargetingSpec : ITargetingSpec
{
    private readonly List<ITargetFilter> _filters = new();

    public Geid Id { get; } = Geid.New;
    public string Description { get; set; }
    public TargetingType Type { get; set; } = TargetingType.Entity;
    public IReadOnlyList<ITargetFilter> Filters => _filters;
    public TargetPriority Priority { get; set; } = TargetPriority.First;
    public int MinTargets { get; set; } = 1;
    public int MaxTargets { get; set; } = 1;
    public SubjectRole TargetRole { get; set; } = SubjectRole.Target;
    public Geid? SourceEntity { get; set; }
    public InsufficientTargetsBehavior OnInsufficientTargets { get; set; } = InsufficientTargetsBehavior.Cancel;
    public Func<EventContext, IGameEvent> AlternativeEffectFactory { get; set; }

    /// <summary>
    /// Добавляет фильтр в список. Все фильтры применяются с AND-логикой.
    /// Возвращает this для поддержки fluent-синтаксиса.
    /// </summary>
    public TargetingSpec AddFilter(ITargetFilter filter)
    {
        if (filter != null)
            _filters.Add(filter);
        return this;
    }
}

public enum TargetingType
{
    None,
    Entity,
    Area,
    Direction,
    Projectile
}
