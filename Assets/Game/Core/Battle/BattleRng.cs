using System;
using System.Collections.Generic;

public sealed class BattleRng
{
    private readonly Random _random;

    public int Seed { get; }

    public BattleRng(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }
    // === Base ===

    /// <summary>
    /// [0, max)
    /// </summary>
    public int NextInt(int max)
    {
        return _random.Next(max);
    }

    /// <summary>
    /// [min, max)
    /// </summary>
    public int NextInt(int min, int max)
    {
        return _random.Next(min, max);
    }

    /// <summary>
    /// [0.0, 1.0)
    /// </summary>
    public float NextFloat()
    {
        return (float)_random.NextDouble();
    }

    /// <summary>
    /// [min, max)
    /// </summary>
    public float NextFloat(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    // === Helpers ===

    public bool RollChance(float chance01)
    {
        if (chance01 <= 0f) return false;
        if (chance01 >= 1f) return true;

        return NextFloat() < chance01;
    }

    public T PickOne<T>(IReadOnlyList<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("Cannot pick from empty list");

        return list[NextInt(list.Count)];
    }
}
