using System.Threading.Tasks;
using UnityEngine;

public sealed class TeleportVisualTask : IVisualTask
{
    private readonly GEID _moverId;
    private readonly HexCoordinates _targetHex;

    public TeleportVisualTask(GEID moverId, HexCoordinates targetHex)
    {
        _moverId = moverId;
        _targetHex = targetHex;
    }

    public async Task PlayAnimationAsync(BattlefieldRenderer renderer)
    {
        var view = renderer.GetView(_moverId);
        if (view != null)
        {
            Vector3 targetPos = renderer.GetWorldPositionForHex(_targetHex);
            view.SetPosition(targetPos);
            
            // Небольшая задержка для визуального фокуса
            await Task.Delay(100);
        }
    }
}
