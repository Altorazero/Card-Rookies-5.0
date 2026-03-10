/// <summary>
/// Определяет порядок сортировки кандидатов перед выбором финального списка целей.
/// </summary>
public enum TargetPriority
{
    /// <summary>Первые N кандидатов в порядке перечисления сущностей (поведение по умолчанию).</summary>
    First,

    /// <summary>Случайный выбор N целей. Использует BattleState.Rng для детерминированности.</summary>
    Random,

    /// <summary>N сущностей с наибольшим текущим HP (по убыванию).</summary>
    HighestHp,

    /// <summary>N сущностей с наименьшим текущим HP (по возрастанию).</summary>
    LowestHp,

    /// <summary>N ближайших к SourceEntity сущностей по гексагональному расстоянию.</summary>
    Nearest,
}
