/// <summary>
/// Луч: гексы, лежащие строго на прямой от центра в заданном направлении
/// на расстоянии от 1 до <see cref="MaxLength"/> шагов включительно.
///
/// <see cref="Direction"/> — единичный гексовый вектор (один из 6 стандартных направлений),
/// например <c>new HexCoordinates(1, 0)</c>.
/// </summary>
public class HexLineShape : IHexShape
{
    public HexCoordinates Direction { get; }
    public int MaxLength { get; }

    public HexLineShape(HexCoordinates direction, int maxLength)
    {
        Direction = direction;
        MaxLength = maxLength;
    }

    public bool Contains(HexCoordinates point, HexCoordinates origin)
    {
        for (int k = 1; k <= MaxLength; k++)
        {
            if (point.Q == origin.Q + Direction.Q * k &&
                point.R == origin.R + Direction.R * k)
                return true;
        }
        return false;
    }
}
