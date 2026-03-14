/// <summary>
/// Событие цикла — создаёт рекурсию из событий.
/// На каждой итерации проверяет <see cref="ILoopState.ShouldContinue"/>,
/// ставит в очередь <see cref="ILoopState.CreateStepEffect"/> и новый
/// <see cref="LoopEvent"/> с состоянием <see cref="ILoopState.Advance"/>.
/// </summary>
public class LoopEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Текущее состояние итерации цикла.</summary>
    public ILoopState State { get; }

    public LoopEvent(Geid systemSourceId, ILoopState state)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        State = state;
    }
}
