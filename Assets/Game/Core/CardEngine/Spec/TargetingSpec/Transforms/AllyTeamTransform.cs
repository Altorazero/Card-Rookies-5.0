using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class AllyTeamTransform<BaseEntity> : ICandidateTransform<IEntity>
{
    public IEnumerable<IEntity> Transform(IEnumerable<IEntity> candidates, ExecutionContext context)
    {
        var caster = (context.EventContext.Event as IHaveSubjects)?.GetFirstSubject(Role.Source);
        if (caster == null) return Enumerable.Empty<IEntity>();

        var casterTeam = caster.GetComponent<TeamComponent>();
        if (casterTeam == null) return Enumerable.Empty<IEntity>();

        return candidates.Where(e => 
        {
            var targetTeam = e.GetComponent<TeamComponent>();
            return targetTeam != null && targetTeam.TeamId == casterTeam.TeamId;
        });
    }
}
