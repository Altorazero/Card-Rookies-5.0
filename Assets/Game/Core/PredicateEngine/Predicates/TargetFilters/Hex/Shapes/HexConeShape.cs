/// <summary>
/// Конус: область вдоль оси <see cref="Direction"/> глубиной <see cref="MaxRadius"/>.
/// На каждой глубине <c>d</c> включаются гексы, находящиеся на расстоянии ≤ <see cref="HalfSpread"/>
/// от осевого гекса (origin + direction * d).
///
/// При <c>HalfSpread = 0</c> — одиночный луч (линия).
/// При <c>HalfSpread = 1</c> — конус шириной 3 гекса на каждой глубине.
///
/// <see cref="Direction"/> — единичный гексовый вектор, например <c>new HexCoordinates(1, 0)</c>.
/// </summary>
public class HexConeShape : IHexShape
{
    public HexCoordinates Direction { get; }
    public int MaxRadius { get; }
    public int HalfSpread { get; }

    public HexConeShape(HexCoordinates direction, int maxRadius, int halfSpread = 1)
    {
        Direction = direction;
        MaxRadius = maxRadius;
        HalfSpread = halfSpread;
    }

    public bool Contains(HexCoordinates point, HexCoordinates origin)
    {
        if (point.Equals(origin)) return false;

        for (int depth = 1; depth <= MaxRadius; depth++)
        {
            var axisHex = new HexCoordinates(
                origin.Q + Direction.Q * depth,
                origin.R + Direction.R * depth);

            if (HexCoordinates.Distance(axisHex, point) <= HalfSpread)
                return true;
        }
        return false;
    }
}
