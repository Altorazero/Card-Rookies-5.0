using System.Collections.Generic;
using UnityEngine;

public enum EventStatus
{
    Pending,     // в очереди, ещЄ не обрабатывалось
    Cancelled,   // запрещено до применени€
    Replaced,    // заменено другим действием
    Fizzled,     // не смогло применитьс€ (нет целей и т.п.)
    Applied      // успешно применено
}

public interface IGameEvent
{
    /// <summary>
    /// —татус событи€. 
    /// Pending - в очереди, ещЄ не обрабатывалось
    /// </summary>
    EventStatus Status { get; set; }

    /// <summary>
    /// јйди самого Event.
    /// </summary>
    Geid Id { get; }
    /// <summary>
    /// Id нечта, вызвавшего по€вление этого Event. Ќужно дл€ системы, чтобы избегать циклов!
    /// </summary>
    Geid SystemSourceId { get; }


}

public class GameEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public GameEvent(Geid systemSourceId)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
    }
}

// »нтерфейс дл€ обобщени€ фаз событий
public interface IPhaseEvent : IGameEvent { }
// ‘азы обработки событий 
public interface IGuardPhaseEvent : IPhaseEvent { }
//public interface IValidatePhaseEvent : IGameEvent { }
public interface IReplacePhaseEvent : IPhaseEvent { }
public interface IModifyPhaseEvent : IPhaseEvent { }
public interface ITargetResolvePhaseEvent : IPhaseEvent { }
public interface IApplyPhaseEvent : IPhaseEvent { }
public interface IAfterPhaseEvent : IPhaseEvent { }
public interface ISBAEvent : IPhaseEvent { }



public enum SubjectRole
{
    Source,
    Target,
    Owner,
    Auxiliary, // вспомогательный участник событи€, не основной
    PrimaryTarget, // главна€ цель событи€
    SecondaryTarget,

}
public sealed class Subject
{
    /// <summary>
    /// јйди сущности-участника событи€.
    /// </summary>
    public Geid Entity { get; set; }
    /// <summary>
    /// –оль участника в событии.
    /// </summary>
    public SubjectRole Role { get; set; }
}
public interface IHaveSubjects : IGameEvent
{
    /// <summary>
    /// —писок участников событи€ с их рол€ми.
    /// </summary>
    public List<Subject> SubjectsList { get; set; }

}