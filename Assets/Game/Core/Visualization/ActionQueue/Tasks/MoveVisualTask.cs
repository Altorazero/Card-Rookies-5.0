using System.Threading.Tasks;
using UnityEngine;

public sealed class MoveVisualTask : IVisualTask
{
    private readonly GEID _moverId;
    private readonly HexCoordinates _targetHex;
    private readonly float _duration;

    public MoveVisualTask(GEID moverId, HexCoordinates targetHex, float duration = 0.3f)
    {
        _moverId = moverId;
        _targetHex = targetHex;
        _duration = duration;
    }

    public async Task PlayAnimationAsync(BattlefieldRenderer renderer)
    {
        var view = renderer.GetView(_moverId);
        if (view != null)
        {
            Vector3 targetPos = renderer.GetWorldPositionForHex(_targetHex);
            
            // Ждем завершения корутины плавного перемещения на View
            await view.MoveToAsync(targetPos, _duration);
        }
    }
}
