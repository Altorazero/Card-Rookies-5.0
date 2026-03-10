using System.Collections.Generic;

public class HealEvent : IGameEvent, IHaveSubjects
{
    public int HealAmount { get; set; }
    public EventStatus Status { get; set; }
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<List<Geid>> Subjects { get; set; }

    public HealEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int healAmount)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        HealAmount = healAmount;
        Subjects = SubjectsHelper.Create(
            (SubjectRole.Source, sourceId),
            (SubjectRole.Target, targetId)
        );
    }
}
