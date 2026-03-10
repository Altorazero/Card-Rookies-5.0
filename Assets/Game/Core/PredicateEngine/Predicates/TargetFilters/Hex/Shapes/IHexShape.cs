/// <summary>
/// Описывает область на гексагональном поле.
/// <see cref="Contains"/> проверяет принадлежность точки к области при заданном центре.
/// </summary>
public interface IHexShape
{
    /// <param name="point">Координаты проверяемого гекса.</param>
    /// <param name="origin">Координаты центра/источника области.</param>
    bool Contains(HexCoordinates point, HexCoordinates origin);
}
