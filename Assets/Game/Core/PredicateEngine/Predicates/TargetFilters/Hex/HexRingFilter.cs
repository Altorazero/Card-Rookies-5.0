/// <summary>
/// Удобный фильтр-обёртка: принимает цели на расстоянии ровно <see cref="Radius"/> гексов
/// от <see cref="OriginEntity"/>. Эквивалентен <c>HexShapeFilter(origin, new HexRingShape(r))</c>.
/// </summary>
public class HexRingFilter : ITargetFilter
{
    public Geid OriginEntity { get; }
    public int Radius { get; }

    private readonly HexShapeFilter _inner;

    public HexRingFilter(Geid originEntity, int radius)
    {
        OriginEntity = originEntity;
        Radius = radius;
        _inner = new HexShapeFilter(originEntity, new HexRingShape(radius));
    }

    public bool IsTargetValid(Geid target, EventContext context)
        => _inner.IsTargetValid(target, context);
}
