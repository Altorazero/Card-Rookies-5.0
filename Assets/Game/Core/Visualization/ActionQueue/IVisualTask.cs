using System.Threading.Tasks;

public interface IVisualTask
{
    /// <summary>
    /// Проигрывает визуальный эффект/анимацию задачи и ожидает её завершения.
    /// </summary>
    Task PlayAnimationAsync(BattlefieldRenderer renderer);
}
