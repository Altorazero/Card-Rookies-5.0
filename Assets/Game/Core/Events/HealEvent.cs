using System.Collections.Generic;

public class HealEvent : IGameEvent, IHaveSubjects
{
    public int HealAmount { get; set; }
    public EventStatus Status { get; set; }
    public Geid Id { get; }
    public Geid SystemSourceId { get; }
    public List<Subject> SubjectsList { get; set; }
    public HealEvent(Geid systemSourceId, Geid sourceId, Geid targetId, int healAmount)
    {
        Id = Geid.New;
        SystemSourceId = systemSourceId;
        HealAmount = healAmount;
        SubjectsList = new List<Subject>
        {
            new Subject { Entity = sourceId, Role = SubjectRole.Source },
            new Subject { Entity = targetId, Role = SubjectRole.Target }
        };
    }
}