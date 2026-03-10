/// <summary>
/// Диск: все гексы на расстоянии ≤ <see cref="MaxRadius"/> от центра (включительно).
/// </summary>
public class HexCircleShape : IHexShape
{
    public int MaxRadius { get; }

    public HexCircleShape(int maxRadius)
    {
        MaxRadius = maxRadius;
    }

    public bool Contains(HexCoordinates point, HexCoordinates origin)
        => HexCoordinates.Distance(point, origin) <= MaxRadius;
}
