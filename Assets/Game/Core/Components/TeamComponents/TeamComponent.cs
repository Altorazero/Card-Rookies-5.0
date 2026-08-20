public class TeamComponent
{
    /// <summary>
    /// Идентификатор команды
    /// </summary>
    public GEID TeamId { get; set; }
    public TeamComponent(GEID teamId)
    {
        TeamId = teamId;
    }
}