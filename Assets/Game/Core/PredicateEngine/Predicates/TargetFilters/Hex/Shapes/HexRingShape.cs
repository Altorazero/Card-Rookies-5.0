/// <summary>
/// Кольцо: только гексы на расстоянии ровно <see cref="Radius"/> от центра.
/// </summary>
public class HexRingShape : IHexShape
{
    public int Radius { get; }

    public HexRingShape(int radius)
    {
        Radius = radius;
    }

    public bool Contains(HexCoordinates point, HexCoordinates origin)
        => HexCoordinates.Distance(point, origin) == Radius;
}
