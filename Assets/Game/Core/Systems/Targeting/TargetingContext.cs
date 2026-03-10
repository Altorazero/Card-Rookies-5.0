using System.Collections.Generic;

/// <summary>
/// Изменяемый контекст, передаваемый через все шаги пайплайна таргетинга.
/// Каждый шаг читает и/или модифицирует список кандидатов и управляющие флаги.
/// </summary>
public class TargetingContext
{
    /// <summary>Текущий рабочий список кандидатов на роль цели.</summary>
    public List<Geid> Candidates { get; } = new();

    /// <summary>Контекст события (BattleState, Dispatcher и т.д.).</summary>
    public EventContext EventContext { get; }

    /// <summary>Событие, запросившее таргетинг.</summary>
    public INeedTargeting TargetingEvent { get; }

    /// <summary>Спецификация таргетинга (содержит TargetRole и список шагов).</summary>
    public ITargetingSpec Spec { get; }

    /// <summary>
    /// Если true, выполнение следующих шагов пайплайна прерывается.
    /// Устанавливается действиями ExitCondition.
    /// </summary>
    public bool Stopped { get; set; }

    /// <summary>
    /// Если true, кандидаты уже записаны в Subjects события одним из шагов пайплайна.
    /// TargetingSystem не выполняет повторную запись.
    /// </summary>
    public bool AlreadyCommitted { get; set; }

    public TargetingContext(EventContext eventContext, INeedTargeting evt, ITargetingSpec spec)
    {
        EventContext = eventContext;
        TargetingEvent = evt;
        Spec = spec;
    }
}
