using System.Collections.Generic;

/// <summary>
/// Событие проверки разыгрывания карты.
/// Guard-фаза: проверяет наличие ресурсов (мана, энергия).
///   - Cancelled — ресурсов недостаточно.
///   - Fizzled   — нет подходящих целей (если карта их требует).
/// After-фаза: если проверка прошла, создаёт и ставит в очередь <see cref="PlayCardEvent"/>.
/// Ссылка на порождённое событие сохраняется в <see cref="SpawnedPlayCardEvent"/>.
/// </summary>
public class PlayCardCheckEvent : IGameEvent, IGuardPhaseEvent, IAfterPhaseEvent, IHaveSubjects
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public Geid Id { get; }
    public Geid SystemSourceId { get; }

    /// <summary>Карта, которую пытаются разыграть.</summary>
    public IPlayingCard Card { get; }

    /// <summary>Разыгрывающая сущность.</summary>
    public Geid CasterId { get; }

    /// <summary>Выбранные цели (может быть пустым для карт без явного выбора цели).</summary>
    public IReadOnlyList<Geid> SelectedTargets { get; }

    /// <summary>Порождённое <see cref="PlayCardEvent"/>. Заполняется системой в After-фазе.</summary>
    public PlayCardEvent SpawnedPlayCardEvent { get; set; }

    public List<List<Geid>> Subjects { get; set; }

    public PlayCardCheckEvent(Geid systemSourceId, IPlayingCard card, Geid casterId, IReadOnlyList<Geid> selectedTargets = null)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        Card = card;
        CasterId = casterId;
        SelectedTargets = selectedTargets ?? System.Array.Empty<Geid>();
        Subjects = SubjectsHelper.Create((SubjectRole.Source, casterId));
    }
}
