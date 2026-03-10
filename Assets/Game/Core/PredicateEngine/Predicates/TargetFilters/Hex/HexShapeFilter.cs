using System;

/// <summary>
/// Фильтр по произвольной гексагональной форме (<see cref="IHexShape"/>).
/// Принимает цель, если её координаты входят в область формы, центрированной на <see cref="OriginEntity"/>.
///
/// Сущности без <see cref="HexComponent"/> (цель или источник) отбрасываются.
/// </summary>
public class HexShapeFilter : ITargetFilter
{
    /// <summary>Сущность, от которой отсчитывается центр формы.</summary>
    public Geid OriginEntity { get; }

    /// <summary>Форма области.</summary>
    public IHexShape Shape { get; }

    public HexShapeFilter(Geid originEntity, IHexShape shape)
    {
        Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        OriginEntity = originEntity;
    }

    public bool IsTargetValid(Geid target, EventContext context)
    {
        var state = context.BattleState;

        var originHex = state.GetEntity(OriginEntity)?.GetComponent<HexComponent>();
        if (originHex == null) return false;

        var targetHex = state.GetEntity(target)?.GetComponent<HexComponent>();
        if (targetHex == null) return false;

        return Shape.Contains(targetHex.Coordinates, originHex.Coordinates);
    }
}
