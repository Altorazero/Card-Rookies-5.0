using System.Collections.Generic;

/// <summary>
/// Действие: немедленно записывает текущих кандидатов как цели в Subjects события
/// и останавливает пайплайн без изменения статуса события.
///
/// Полезно при многоуровневом пайплайне, когда нужно зафиксировать промежуточный результат
/// до перехода к следующему блоку шагов.
/// </summary>
public class CommitAndStopAction : ITargetingAction
{
    public void Execute(TargetingContext context)
    {
        var evt = context.TargetingEvent;
        int roleIdx = (int)context.Spec.TargetRole;

        evt.EnsureSubjects();
        while (evt.Subjects.Count <= roleIdx)
            evt.Subjects.Add(new List<Geid>());

        foreach (var id in context.Candidates)
            evt.Subjects[roleIdx].Add(id);

        context.AlreadyCommitted = true;
        context.Stopped = true;
    }
}
