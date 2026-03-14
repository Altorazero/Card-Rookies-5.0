using System.Collections.Generic;

/// <summary>
/// Событие разыгрывания карты — для аур и пассивок.
/// Apply-фаза: ставит в очередь все эффекты карты (<see cref="IPlayingCard.Effects"/>).
/// </summary>
public class PlayCardEvent : IGameEvent, IApplyPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Разыгрываемая карта.</summary>
    public IPlayingCard Card { get; }

    /// <summary>Разыгрывающая сущность.</summary>
    public Geid CasterId { get; }

    /// <summary>Выбранные цели.</summary>
    public IReadOnlyList<Geid> SelectedTargets { get; }

    public List<List<Geid>> Subjects { get; set; }

    public PlayCardEvent(Geid systemSourceId, IPlayingCard card, Geid casterId, IReadOnlyList<Geid> selectedTargets = null)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        Card = card;
        CasterId = casterId;
        SelectedTargets = selectedTargets ?? System.Array.Empty<Geid>();
        Subjects = SubjectsHelper.Create((SubjectRole.Source, casterId));
    }
}
