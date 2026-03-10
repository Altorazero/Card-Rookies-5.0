/// <summary>
/// Определяет, что делать, если найдено меньше целей, чем требует MinTargets.
/// </summary>
public enum InsufficientTargetsBehavior
{
    /// <summary>
    /// Отменить событие (Fizzle). Поведение по умолчанию.
    /// </summary>
    Cancel,

    /// <summary>
    /// Продолжить с найденными целями (даже если их 0).
    /// Targets заполняются тем, что удалось найти.
    /// </summary>
    UseFound,

    /// <summary>
    /// Продолжить, не заполняя Subjects («выстрел в пустоту»).
    /// Событие не отменяется, но получатель получит пустой список целей.
    /// </summary>
    ShootVoid,

    /// <summary>
    /// Отменить исходное событие и создать альтернативное через
    /// <see cref="ITargetingSpec.AlternativeEffectFactory"/>.
    /// Если фабрика не задана — событие просто отменяется.
    /// </summary>
    AlternativeEffect,
}
