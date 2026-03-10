/// <summary>
/// Удобный фильтр-обёртка: принимает цели в гексагональном радиусе ≤ <see cref="MaxRadius"/>
/// от <see cref="OriginEntity"/>. Эквивалентен <c>HexShapeFilter(origin, new HexCircleShape(r))</c>.
/// </summary>
public class HexRadiusFilter : ITargetFilter
{
    public Geid OriginEntity { get; }
    public int MaxRadius { get; }

    private readonly HexShapeFilter _inner;

    public HexRadiusFilter(Geid originEntity, int maxRadius)
    {
        OriginEntity = originEntity;
        MaxRadius = maxRadius;
        _inner = new HexShapeFilter(originEntity, new HexCircleShape(maxRadius));
    }

    public bool IsTargetValid(Geid target, EventContext context)
        => _inner.IsTargetValid(target, context);
}
