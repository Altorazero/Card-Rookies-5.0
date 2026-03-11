/// <summary>
/// Фильтр: принимает только сущности, принадлежащие ЧУЖОЙ команде (враги).
/// Сравнивает TeamId источника события с TeamId цели.
/// Сущности без TeamComponent или без источника отбрасываются.
/// </summary>
public class EnemyTeamFilter : ITargetFilter
{
    public bool IsTargetValid(Geid target, EventContext context)
    {
        if (context?.Event is not IHaveSubjects eventWithSubjects) return false;

        var sourceId = eventWithSubjects.GetFirstSubject(SubjectRole.Source);
        if (sourceId == Geid.Empty) return false;

        var sourceEntity = context.BattleState.GetEntity(sourceId);
        var targetEntity = context.BattleState.GetEntity(target);

        if (sourceEntity == null || targetEntity == null) return false;

        var sourceTeam = sourceEntity.GetComponent<TeamComponent>();
        var targetTeam = targetEntity.GetComponent<TeamComponent>();

        if (sourceTeam == null || targetTeam == null) return false;

        return sourceTeam.TeamId != targetTeam.TeamId;
    }
}
