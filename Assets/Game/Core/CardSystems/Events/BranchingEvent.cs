using System.Collections.Generic;

/// <summary>
/// Событие ветвления — дерево условий и событий.
/// Перебирает ветви <see cref="Branches"/> и ставит в очередь эффекты тех, чьё условие выполнено.
/// <para>
/// <see cref="ExecuteLimit"/> = 0 — выполнить все сработавшие ветви,
/// <see cref="ExecuteLimit"/> > 0 — не более N ветвей.
/// </para>
/// Если ни одна ветвь не сработала и задан <see cref="DefaultEffect"/> — выполняется он.
/// </summary>
public class BranchingEvent : IGameEvent, IApplyPhaseEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Список ветвей (условие + событие).</summary>
    public IReadOnlyList<BranchEntry> Branches { get; }

    /// <summary>0 = выполнить все сработавшие; &gt;0 = не более N.</summary>
    public int ExecuteLimit { get; }

    /// <summary>Выполняется, если ни одна ветвь не сработала. Может быть null.</summary>
    public IGameEvent DefaultEffect { get; }

    public BranchingEvent(Geid systemSourceId, IReadOnlyList<BranchEntry> branches, int executeLimit = 0, IGameEvent defaultEffect = null)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        Branches = branches;
        ExecuteLimit = executeLimit;
        DefaultEffect = defaultEffect;
    }
}
