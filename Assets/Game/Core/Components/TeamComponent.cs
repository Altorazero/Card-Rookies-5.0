public class TeamComponent
{
    /// <summary>
    /// Идентификатор команды
    /// </summary>
    public Geid TeamId { get; set; }
    public TeamComponent(Geid teamId)
    {
        TeamId = teamId;
    }
}