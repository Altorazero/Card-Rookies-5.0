using System.ComponentModel;


/// <summary>
/// Позиция сущности на гекс-поле. Неизменяемый компонент —
/// перемещение сущности заменяет весь компонент, а не мутирует поле Coordinates.
/// </summary>
public sealed record HexComponent(HexCoordinates Coordinates) : IComponent;



namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
