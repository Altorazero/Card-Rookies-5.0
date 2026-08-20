using System.Threading.Tasks;

public sealed class DamageVisualTask : IVisualTask
{
    private readonly GEID _targetId;
    private readonly int _amount;

    public DamageVisualTask(GEID targetId, int amount)
    {
        _targetId = targetId;
        _amount = amount;
    }

    public async Task PlayAnimationAsync(BattlefieldRenderer renderer)
    {
        var view = renderer.GetView(_targetId);
        if (view != null)
        {
            // Ждем завершения тряски/урона самого существа
            await view.PlayDamageAnimationAsync(_amount);
        }
    }
}
