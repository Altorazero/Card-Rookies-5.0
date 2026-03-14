/// <summary>
/// Состояние одного шага цикла.
/// Хранит данные итерации (например, текущая сила лечения, список посещённых целей)
/// и умеет порождать эффект шага и следующее состояние.
/// </summary>
public interface ILoopState
{
    /// <summary>Возвращает true, если цикл должен продолжаться.</summary>
    bool ShouldContinue(EventContext context);

    /// <summary>Создаёт GameEvent для текущего шага цикла.</summary>
    IGameEvent CreateStepEffect(EventContext context);

    /// <summary>
    /// Возвращает состояние для следующей итерации.
    /// Возвращает null, если следующая итерация невозможна (цикл завершается после текущего шага).
    /// </summary>
    ILoopState Advance(EventContext context);
}
